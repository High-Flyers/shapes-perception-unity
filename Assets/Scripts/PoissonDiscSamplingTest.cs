using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PoissonDiscSamplingTest
{
    public static List<Vector2> GeneratePoints(float radius, List<Vector2> polygon,
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

        float cellSize = radius / Mathf.Sqrt(2);
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize),
            Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        var points = new List<Vector2>();
        var spawnPoints = new List<Vector2>();

        float x = Random.Range(0, sampleRegionSize.x);
        float y = Random.Range(0, sampleRegionSize.y);
        
        spawnPoints.Add(new Vector2(x, y));
        
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + dir * Random.Range(radius, 2 * radius);
                
                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int) (candidate.x / cellSize), (int) (candidate.y / cellSize)] = points.Count;
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
            points[i] += center;
        
        if (points.Count() == 0)
            points.Add(new Vector2(x - sampleRegionSize.x / 2, y - sampleRegionSize.y / 2));
        
        return points;
    }

    static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points,
        int[,] grid)
    {
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 &&
            candidate.y < sampleRegionSize.y)
        {
            int cellX = (int) (candidate.x / cellSize);
            int cellY = (int) (candidate.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
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
}