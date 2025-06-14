﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;


public class PointsInCameraViewGen
{
    private float maxDistance;
    private Camera camera;
    private List<float[]> props;
    private float offset;

    public PointsInCameraViewGen(List<float[]> props, Camera camera, float maxDistance, float offset)
    {
        this.props = props;
        this.camera = camera;
        this.maxDistance = maxDistance;
        this.offset = offset;
    }

    public List<Point> generatePoints()
    {
        var points = new List<Point>();
        var height = camera.transform.position.y;
        var cameraAngles = camera.transform.eulerAngles;

        // Bottom  angle of field of view (0 when parallel to ground)
        var angleBottom = cameraAngles.x + camera.fieldOfView / 2;
        // Top angle of field of view
        var angleTop = cameraAngles.x - camera.fieldOfView / 2;

        var polygon = new List<Vector3>();

        // Bottom ray of FOV(field of view) hits the ground - polygon points could be calculated
        if (angleBottom > 0)
        {
            Vector3 leftBottom = Vector3.zero,
                rightBottom = Vector3.zero,
                leftTop = Vector3.zero,
                rightTop = Vector3.zero;

            var depthBottom = getEdgeLength(Mathf.Abs(90 - angleBottom) * Mathf.Deg2Rad, height,
                getTriangleAngle(0)); //bottom FOV edge length
            var depthTop = getEdgeLength(Mathf.Abs(90 - angleTop) * Mathf.Deg2Rad, height,
                getTriangleAngle(0)); //top FOV edge length

            // Point in the center of bottom closing FOV edge
            var centerBottom = getPoint(new Vector3(0.5f, 0, maxDistance));
            // Point in the center of top closing FOV edge 
            var centerTop = getPoint(new Vector3(0.5f, 1, maxDistance));

            // Get bottom polygon points basing on full camera FOV
            if (centerBottom.y < 0)
            {
                leftBottom = getPoint(new Vector3(0, 0, depthBottom));
                rightBottom = getPoint(new Vector3(1, 0, depthBottom));
            }
            else
            {
                // Get bottom polygon points basing on maxDistance, when angleBottom < 90 there is no way to spawn them
                if (angleBottom > 90)
                    getPointsByMaxDist(out leftBottom, out rightBottom, height, -1);
            }

            // Get top polygon points basing on full camera FOV 
            if (centerTop.y < 0)
            {
                leftTop = getPoint(new Vector3(0, 1, depthTop));
                rightTop = getPoint(new Vector3(1, 1, depthTop));
            }
            else
            {
                // Get top polygon points basing on maxDistance
                if (centerBottom.y < 0 || angleBottom > 90)
                    getPointsByMaxDist(out leftTop, out rightTop, height, 1);
            }

            // Add points to polygon
            foreach (var point in new List<Vector3> {leftBottom, rightBottom, rightTop, leftTop})
            {
                if (point != Vector3.zero)
                    polygon.Add(point);
            }

            var polygon2D = new List<Vector2>();

            foreach (var vector in polygon)
                polygon2D.Add(new Vector2(vector.x, vector.z));

            points = PoissonDiscSamplingPolygon.GeneratePoints(polygon2D, offset, props);
        }

        return points;
    }


    /// Get point, which is distant from camera basing on viewport direction 
    /// viewport - x,y - camera viewport, z- relative distance from camera
    private Vector3 getPoint(Vector3 viewport)
    {
        Vector3 direction = camera.ViewportPointToRay(viewport).direction;
        Debug.DrawRay(camera.transform.position, direction * viewport.z, Color.green);
        var point = camera.transform.position + direction * viewport.z;

        return point;
    }


    /// Get half of vertical angle of FOV - based on y viewport coordinate
    /// y - y coordinate of FOV 
    private float getTriangleAngle(float y)
    {
        var cameraPosition = camera.transform.position;
        var centerPoint = camera.ViewportToWorldPoint(new Vector3(0.5f, y, 1));
        var rightPoint = camera.ViewportToWorldPoint(new Vector3(1, y, 1));
        var h = Vector3.Distance(cameraPosition, centerPoint);
        var c = Vector3.Distance(cameraPosition, rightPoint);

        return Mathf.Acos(h / c);
    }

    /// Get edge length of useful part of FOV
    /// angle - angle from vertical line to middle of FOV face
    /// height - camera height
    /// triangleAngle - half of vertical angle of FOV - based on y viewport
    private float getEdgeLength(float angle, float height, float triangleAngle)
    {
        var edgeLen = height / Mathf.Cos(angle);
        edgeLen = edgeLen / Mathf.Cos(triangleAngle);

        return edgeLen;
    }

    /// Get polygon points basing on max distance
    /// leftPoint
    /// rightPoint
    /// height - height of camera
    /// edgeNum - bottom(-1) or top(1) edge of polygon
    private void getPointsByMaxDist(out Vector3 leftPoint, out Vector3 rightPoint, float height, int edgeNum)
    {
        var z = edgeNum * Mathf.Sqrt(Mathf.Pow(maxDistance, 2) - Mathf.Pow(height, 2));
        var pointOnScreen = camera.WorldToViewportPoint(new Vector3(0, 0, z));

        var angle = Mathf.Acos(height / maxDistance);
        var edgeLen = getEdgeLength(angle, height, getTriangleAngle(pointOnScreen.y));

        leftPoint = getPoint(new Vector3(0, pointOnScreen.y, edgeLen));
        rightPoint = getPoint(new Vector3(1, pointOnScreen.y, edgeLen));
    }
}