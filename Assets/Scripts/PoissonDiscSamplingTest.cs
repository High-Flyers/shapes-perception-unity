using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Structures;
using UnityEngine;
using Random = UnityEngine.Random;


public static class PoissonDiscSamplingTest
{
    public static List<Point> GeneratePoints(float offset, List<Vector2> polygon, List<float[]> sizes,
        int numSamplesBeforeRejection = 30)
    {
        var sampleRegionSize = Vector2.zero;
        var minX = polygon.Min(p => p.x);
        var minY = polygon.Min(p => p.y);

        for (int i = 0; i < polygon.Count(); i++)
            polygon[i] = new Vector2(polygon[i].x - minX, polygon[i].y - minY);

        sampleRegionSize.x = polygon.Max(p => p.x);
        sampleRegionSize.y = polygon.Max(p => p.y);

        var center = new Vector2(sampleRegionSize.x / 2 + minX, sampleRegionSize.y / 2 + minY);

        float minSize = sizes.Min(s => s[0]);

        float cellSize = (minSize + offset) / Mathf.Sqrt(2);
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize),
            Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        var pointProps = CreatePointsProps(sizes, offset, cellSize);
        var points = new List<Point>();
        var spawnPoints = new List<Point>();

        float x = sampleRegionSize.x / 2;   //Random.Range(0, sampleRegionSize.x);
        float y = sampleRegionSize.y / 2;   // Random.Range(0, sampleRegionSize.y);
        
        float sumProbs = CumSumProbs(ref pointProps);
        int propIndex = SamplePointPropsIndex(sumProbs, pointProps);
        
        spawnPoints.Add(new Point(new Vector2(x, y), propIndex));
        
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Point spawnPoint = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

                propIndex = SamplePointPropsIndex(sumProbs, pointProps);
                var candidateProps = pointProps[propIndex];
                var spawnProps = pointProps[spawnPoint.propId];
                var radius = Mathf.Max(candidateProps.radius, spawnProps.radius);
                var candidatePos = spawnPoint.position + dir * Random.Range(radius, 2 * radius);
                var candidate = new Point(candidatePos, propIndex);

                if (IsValid(candidate, pointProps, polygon, cellSize, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int) (candidate.position.x / cellSize), (int) (candidate.position.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }

            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        center -= sampleRegionSize / 2;

        for (int i = 0; i < points.Count; i++)
            points[i].position += center;

        if (points.Count() == 0)
        {
            var pointPositon = new Vector2(x - sampleRegionSize.x / 2, y - sampleRegionSize.y / 2);
            points.Add(new Point(pointPositon, SamplePointPropsIndex(sumProbs, pointProps)));
        }

        return points;
    }

    static bool IsValid(Point candidate, List<PointProps> pointProps, List<Vector2> polygon, float cellSize, List<Point> points,
        int[,] grid)
    {
        if (ContainsPoint(polygon, candidate.position))
        {
            int cellX = (int) (candidate.position.x / cellSize);
            int cellY = (int) (candidate.position.y / cellSize);
            var candidateProps = pointProps[candidate.propId];
            
            int searchStartX = Mathf.Max(0, cellX - candidateProps.gridsToSearch);
            int searchEndX = Mathf.Min(cellX + candidateProps.gridsToSearch, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - candidateProps.gridsToSearch);
            int searchEndY = Mathf.Min(cellY + candidateProps.gridsToSearch, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        float sqrDst = (candidate.position - points[pointIndex].position).sqrMagnitude;
                        var neighbourPointProps = pointProps[points[pointIndex].propId];
                        var radius = Mathf.Max(candidateProps.radius, neighbourPointProps.radius);
                            
                        if (sqrDst < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        return false;
    }
    
    private static bool ContainsPoint(List<Vector2> polyPoints, Vector2 p)
    {
        var j = polyPoints.Count() - 1;
        var inside = false;
        for (int i = 0; i < polyPoints.Count(); j = i++)
        {
            var pi = polyPoints[i];
            var pj = polyPoints[j];
            if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                inside = !inside;
        }
        return inside;
    }

    private static float CumSumProbs(ref List<PointProps> pointProps)
    {
        float sum = 0;
        
        for (int i = 0; i < pointProps.Count(); i++)
        {
            sum += pointProps[i].prob;
            pointProps[i].prob = sum;
        }

        return sum;
    }

    private static int SamplePointPropsIndex(float maxVal, List<PointProps> pointProps)
    {
        float randVal = Random.Range(0, maxVal);
        for (int i = 0; i < pointProps.Count(); i++)
        {
            if (randVal <= pointProps[i].prob)
                return i;
        }

        return -1;
    }

    private static List<PointProps> CreatePointsProps(List<float[]> props, float offset, float cellSize)
    {
        var pointProps = new List<PointProps>();

        for (int i = 0; i < props.Count(); i++)
        {
            props[i][0] += offset;
            pointProps.Add(new PointProps(props[i], cellSize));
        }

        return pointProps;
    }
}