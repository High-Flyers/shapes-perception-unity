using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTemplateProjects
{
    public class PoissonTest : MonoBehaviour
    {
        public float maxDistance = 80;
        public float radius = 3;

        private new Camera camera;
        private List<Vector2> points;
        private List<Vector3> polygon;

        private void Start()
        {
            camera = Camera.main;
            Debug.Log(camera);
        }

        private void OnValidate()
        {
            Debug.Log("xDDDD");
            var height = camera.transform.position.y;
            var cameraAngles = camera.transform.eulerAngles;
            var angleBottom = cameraAngles.x + camera.fieldOfView / 2;
            var angleTop = cameraAngles.x - camera.fieldOfView / 2;
            polygon = new List<Vector3>();

            if (angleBottom > 0)
            {
                Vector3 leftBottom = Vector3.zero,
                    rightBottom = Vector3.zero,
                    leftTop = Vector3.zero,
                    rightTop = Vector3.zero;

                var depthBottom = getEdgeLength(Mathf.Abs(90 - angleBottom) * Mathf.Deg2Rad, height,
                    getTriangleAngle(0));
                var depthTop = getEdgeLength(Mathf.Abs(90 - angleTop) * Mathf.Deg2Rad, height, getTriangleAngle(0));

                var centerBottom = getPoint(new Vector3(0.5f, 0, maxDistance));
                var centerTop = getPoint(new Vector3(0.5f, 1, maxDistance));

                if (centerBottom.y < 0)
                {
                    leftBottom = getPoint(new Vector3(0, 0, depthBottom));
                    rightBottom = getPoint(new Vector3(1, 0, depthBottom));
                }
                else
                {
                    if (angleBottom > 90)
                    {
                        var z = -Mathf.Sqrt(Mathf.Pow(maxDistance, 2) - Mathf.Pow(height, 2));
                        var pointOnScreen = camera.WorldToViewportPoint(new Vector3(0, 0, z));

                        var angle = Mathf.Acos(height / maxDistance);
                        var edgeLen = getEdgeLength(angle, height, getTriangleAngle(pointOnScreen.y));

                        leftBottom = getPoint(new Vector3(0, pointOnScreen.y, edgeLen));
                        rightBottom = getPoint(new Vector3(1, pointOnScreen.y, edgeLen));
                    }
                }

                if (centerTop.y < 0)
                {
                    leftTop = getPoint(new Vector3(0, 1, depthTop));
                    rightTop = getPoint(new Vector3(1, 1, depthTop));
                }
                else
                {
                    if (centerBottom.y < 0 || angleBottom > 90)
                    {
                        var z = Mathf.Sqrt(Mathf.Pow(maxDistance, 2) - Mathf.Pow(height, 2));
                        var pointOnScreen = camera.WorldToViewportPoint(new Vector3(0, 0, z));

                        var angle = Mathf.Acos(height / maxDistance);
                        var edgeLen = getEdgeLength(angle, height, getTriangleAngle(pointOnScreen.y));

                        leftTop = getPoint(new Vector3(0, pointOnScreen.y, edgeLen));
                        rightTop = getPoint(new Vector3(1, pointOnScreen.y, edgeLen));
                    }
                }

                if (leftBottom != Vector3.zero)
                    polygon.Add(leftBottom);
                if (rightBottom != Vector3.zero)
                    polygon.Add(rightBottom);
                if (rightTop != Vector3.zero)
                    polygon.Add(rightTop);
                if (leftTop != Vector3.zero)
                    polygon.Add(leftTop);

                var polygon2D = new List<Vector2>();
                Debug.Log(polygon.Count());

                foreach (var vector in polygon)
                    polygon2D.Add(new Vector2(vector.x, vector.z));

                points = PoissonDiscSamplingTest.GeneratePoints(8, polygon2D);

                Debug.Log(points.Count());
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (polygon != null && polygon.Count() >= 2)
            {
                for (int i = 0; i < polygon.Count(); i++)
                {
                    Gizmos.color = Color.red;

                    if (i < polygon.Count() - 1)
                        Gizmos.DrawLine(polygon[i], polygon[i + 1]);
                    else
                        Gizmos.DrawLine(polygon[i], polygon[0]);
                }

                if (points != null)
                {
                    foreach (var point in points)
                    {
                        var center = new Vector3(point.x, 0, point.y);
                        Gizmos.DrawSphere(center, radius);
                    }
                }
            }
        }

        private Vector3 getPoint(Vector3 viewport)
        {
            Vector3 direction = camera.ViewportPointToRay(viewport).direction;
            Debug.DrawRay(camera.transform.position, direction * viewport.z, Color.green);
            var point = camera.transform.position + direction * viewport.z;

            return point;
        }


        private float getTriangleAngle(float y)
        {
            var cameraPosition = camera.transform.position;
            var centerPoint = camera.ViewportToWorldPoint(new Vector3(0.5f, y, 1));
            var rightPoint = camera.ViewportToWorldPoint(new Vector3(1, y, 1));
            var h = Vector3.Distance(cameraPosition, centerPoint);
            var c = Vector3.Distance(cameraPosition, rightPoint);

            return Mathf.Acos(h / c);
        }

        private float getEdgeLength(float angle, float height, float triangleAngle)
        {
            var edgeLen = height / Mathf.Cos(angle);
            edgeLen = edgeLen / Mathf.Cos(triangleAngle);

            return edgeLen;
        }
    }
}