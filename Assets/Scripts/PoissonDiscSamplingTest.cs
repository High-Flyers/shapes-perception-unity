using System;
using System.Collections.Generic;
using System.Linq;
using Structures;
using UnityEngine;
using Random = UnityEngine.Random;


public static class PoissonDiscSamplingTest
{
    /// <summary>
    ///     Function to generate random points using poisson disc sampling method
    /// </summary>
    /// <param name="polygon">List of 2D points polygon points</param>
    /// <param name="offset">Min distance to other points within specific size</param>
    /// <param name="props">List of float array {radius, probs}</param>
    /// <param name="numSamplesBeforeRejection">Number of max trials getting candidate point before stopping </param>
    /// <returns>Random list of Points</returns>
    public static List<Point> GeneratePoints(List<Vector2> polygon, float offset, List<float[]> props,
        int numSamplesBeforeRejection = 30)
    {
        var propsCopy = props.Select(p => (float[])p.Clone()).ToList();
        
        // Calc sample region size and move polygon to (0, 0)
        var sampleRegionSize = Vector2.zero;
        var minX = polygon.Min(p => p.x);
        var minY = polygon.Min(p => p.y);

        for (int i = 0; i < polygon.Count(); i++)
            polygon[i] = new Vector2(polygon[i].x - minX, polygon[i].y - minY);

        sampleRegionSize.x = polygon.Max(p => p.x);
        sampleRegionSize.y = polygon.Max(p => p.y);

        // Calc original center of polygon
        var center = new Vector2(sampleRegionSize.x / 2 + minX, sampleRegionSize.y / 2 + minY);
        center -= sampleRegionSize / 2;

        // Cumulative sum of probs and adds offset to radius
        float sumProbs = 0;
        propsCopy = propsCopy.Select(p =>
        {
            sumProbs += p[1];
            p[0] += offset;
            p[1] = sumProbs;

            return p;
        }).ToList();

        // Calc cellSize from min radius and grid,
        // which will be used for searching free spaces for candidate point
        float minRadius = propsCopy.Min(p => p[0]);
        float cellSize = minRadius / Mathf.Sqrt(2);
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize),
            Mathf.CeilToInt(sampleRegionSize.y / cellSize)];

        var pointProps = CreatePointsProps(propsCopy, cellSize);
        var points = new List<Point>();
        var spawnPoints = new List<Point>();

        // Get starting spawn point
        float x = sampleRegionSize.x / 2;
        float y = sampleRegionSize.y / 2;
        int startPropsId = SamplePointPropsId(sumProbs, pointProps);
        spawnPoints.Add(new Point(new Vector2(x, y), startPropsId));

        while (spawnPoints.Count > 0)
        {
            // Get current random spawn point
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Point spawnPoint = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                // Get random direction from spawn point
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

                // Create candidate point
                var candidatePropsId = SamplePointPropsId(sumProbs, pointProps);
                var candidateProps = pointProps[candidatePropsId];
                var spawnProps = pointProps[spawnPoint.propId];
                var radius = Mathf.Max(candidateProps.radius, spawnProps.radius);
                var candidatePos = spawnPoint.position + dir * Random.Range(radius, 2 * radius);
                var candidate = new Point(candidatePos, candidatePropsId);

                if (IsValid(candidate, pointProps, polygon, cellSize, points, grid, offset))
                {
                    // Add candidate point to points and spawn points list
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int) (candidate.position.x / cellSize), (int) (candidate.position.y / cellSize)] =
                        points.Count;
                    candidateAccepted = true;
                    break;
                }
            }

            // If none candidate was found remove current spawn point
            if (!candidateAccepted)
                spawnPoints.RemoveAt(spawnIndex);
        }

        // Move points to original center
        for (int i = 0; i < points.Count; i++)
            points[i].position += center;

        // If none point was found create smallest one in the middle
        if (points.Count() == 0)
        {
            var pointPositon = new Vector2(x - sampleRegionSize.x / 2, y - sampleRegionSize.y / 2);
            int minRadiusPropID = propsCopy.FindIndex(p => Math.Abs(p[0] - minRadius) < 0.0001);
            points.Add(new Point(pointPositon, minRadiusPropID));
        }

        return points;
    }

    // Checking if candidate point fulfill conditions
    static bool IsValid(Point candidate, List<PointProps> pointProps, List<Vector2> polygon, float cellSize,
        List<Point> points, int[,] grid, float offset)
    {
        var candidateProps = pointProps[candidate.propId];
        
        if (ContainsPoint(polygon, candidate.position) && IsInsideOfPolygon(polygon, candidate.position, candidateProps.radius - offset))
        {
            // Getting cell coordinates of candidate point and candidate pointProps
            int cellX = (int) (candidate.position.x / cellSize);
            int cellY = (int) (candidate.position.y / cellSize);

            // Getting search area
            int searchStartX = Mathf.Max(0, cellX - candidateProps.gridsToSearch);
            int searchEndX = Mathf.Min(cellX + candidateProps.gridsToSearch, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - candidateProps.gridsToSearch);
            int searchEndY = Mathf.Min(cellY + candidateProps.gridsToSearch, grid.GetLength(1) - 1);

            // Searching area for other points
            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        // Checking if candidate is in proper distance to neighbour point
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

    // Check if point is inside of polygon
    private static bool ContainsPoint(List<Vector2> polygon, Vector2 p)
    {
        var j = polygon.Count() - 1;
        var inside = false;
        for (int i = 0; i < polygon.Count(); j = i++)
        {
            var pi = polygon[i];
            var pj = polygon[j];
            if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                inside = !inside;
        }

        return inside;
    }
    
    // Check if point fits inside polygon by checking
    private static bool IsInsideOfPolygon(List<Vector2> polygon, Vector2 p, float radius)
    {
        for (int i = 0; i < polygon.Count(); i++)
        {
            var p1 = polygon[i];
            var p2 = polygon[0];
            
            if (i < polygon.Count() - 1)
            {
                p1 = polygon[i];
                p2 = polygon[i + 1];
            }
            
            float dist = 0;
            float r = Vector2.Dot(p2 - p1, p - p1);
            
            r /= Mathf.Pow((p2 - p1).magnitude, 2);

            if (r < 0)
                dist = (p - p1).magnitude;
            else if (r > 1)
                dist = (p2 - p).magnitude;
            else
                dist = Mathf.Sqrt(Mathf.Pow((p - p1).magnitude, 2) - Mathf.Pow(r * (p2 - p1).magnitude, 2));

            if (dist <= radius)
                return false;
        }

        return true;
    }

    // Get random id of pointProps
    private static int SamplePointPropsId(float sumProbs, List<PointProps> pointProps)
    {
        float randVal = Random.Range(0, sumProbs);

        for (int i = 0; i < pointProps.Count(); i++)
            if (randVal <= pointProps[i].prob)
                return i;

        return -1;
    }

    private static List<PointProps> CreatePointsProps(List<float[]> props, float cellSize)
    {
        var pointProps = new List<PointProps>();

        for (int i = 0; i < props.Count(); i++)
            pointProps.Add(new PointProps(props[i], cellSize));

        return pointProps;
    }
}