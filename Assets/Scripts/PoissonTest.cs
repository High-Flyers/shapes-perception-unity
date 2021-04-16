using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityTemplateProjects
{
    public class PoissonTest : MonoBehaviour
    {
        public float scale = 1f;

        private new Camera camera;
        private List<Point> points;
        private List<float[]> props = new List<float[]>();
        private PointsInCameraViewGen pointsGen;

        private void Start()
        {
            camera = Camera.main;

            props.Add(new[] {3f, 0.05f});
            props.Add(new[] {1f, 0.95f});
            pointsGen = new PointsInCameraViewGen(props, camera, 80, 3);
        }

        private void OnValidate()
        {
            points = pointsGen.generatePoints();
        }

        // Draw spawning area and spheres in generated points
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (points != null)
            {
                foreach (var point in points)
                {
                    var center = new Vector3(point.position.x, 0, point.position.y);
                    Gizmos.DrawSphere(center, props[point.propId][0] * scale);
                }
            }
        }
    }
}