using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTemplateProjects
{
    public class PoissonTest : MonoBehaviour
    {
        public float maxDistance = 80;

        private new Camera camera;

        private void Start()
        {
            camera = Camera.main;
            Debug.Log(camera);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            var height = camera.transform.position.y;
            var cameraAngles = camera.transform.eulerAngles;
            var polygon = new List<Vector3>();
            var angleBottom = cameraAngles.x + camera.fieldOfView / 2;
            var triangleAngle = getTriangleAngle();
            
            if (angleBottom > 0)
            {
                var depth = height / Mathf.Cos(Mathf.Abs(90 - angleBottom) * Mathf.Deg2Rad);
                depth = depth / Mathf.Cos(triangleAngle);
                
                var centerBottom = getPoint(new Vector3(0.5f, 0, maxDistance));
                var centerTop = getPoint(new Vector3(0.5f, 1, maxDistance));
                var leftBottom = new Vector3(0, 0, 0);
                var rightBottom = new Vector3(0, 0, 0);

                if (centerBottom.y < 0)
                {
                    leftBottom = getPoint(new Vector3(0, 0, depth));
                    rightBottom = getPoint(new Vector3(1, 0, depth));
                }
                else
                {
                    if (angleBottom > 90)
                    {
                        var z = -Mathf.Sqrt(Mathf.Pow(maxDistance, 2) - Mathf.Pow(height, 2));
                        var pointOnScreen = camera.WorldToViewportPoint(new Vector3(0, 0, z));

                        leftBottom = camera.ViewportToWorldPoint(new Vector3(0, pointOnScreen.y, maxDistance));
                        rightBottom = camera.ViewportToWorldPoint(new Vector3(1, pointOnScreen.y, maxDistance));
                    }
                }

                polygon.Add(leftBottom);
                polygon.Add(rightBottom);
            }

            if (polygon.Count() >= 2)
            {
                for (int i = 0; i < polygon.Count(); i++)
                {
                    Gizmos.color = Color.red;

                    if (i < polygon.Count() - 1)
                        Gizmos.DrawLine(polygon[i], polygon[i + 1]);
                    else
                        Gizmos.DrawLine(polygon[i], polygon[0]);
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

        private float getTriangleAngle()
        {
            var cameraPosition = camera.transform.position;
            var centerPoint = camera.ViewportToWorldPoint(new Vector3(0.5f, 0, 1));
            var rightPoint = camera.ViewportToWorldPoint(new Vector3(1, 0, 1));
            var h = Vector3.Distance(cameraPosition, centerPoint);
            var c = Vector3.Distance(cameraPosition, rightPoint);

            return Mathf.Acos(h / c);
        }
    }
    
}