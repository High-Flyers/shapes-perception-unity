using System;
using System.Collections.Generic;
using System.Linq;
using Structures;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Randomizers.Utilities;
using Random = UnityEngine.Random;

namespace Perception.Randomizers
{
    [Serializable]
    [AddRandomizerMenu("Perception/Randomizers/ObjectsInCameraViewRandomizer")]
    public class ObjectsPlacementRandomizer : Randomizer
    {
        public GameObjectParameter prefabs;
        public int randomObjectsCount = 20;
        public float radius = 10;
        public Vector2 regionSize = Vector2.one * 100;
        public int rejectionSamples = 30;
        public int newPointsIteration = 50;

        [Range(0, 100)] public float maxDistance = 40;
        public float maxLabelingDistance = 100;
        public MinMaxParameter height = new MinMaxParameter(5, 40);
        public MinMaxParameter rotation = new MinMaxParameter(30, 150);

        private GameObject container;
        private GameObjectOneWayCache gameObjectOneWayCache;
        private List<GameObject> simObjects;
        private List<Vector2> points;

        protected override void OnAwake()
        {
            var prefabsObjects = prefabs.categories.Select((element) => element.Item1).ToArray();

            simObjects = new List<GameObject>(randomObjectsCount);
            container = new GameObject("Foreground Objects");
            container.transform.parent = scenario.transform;
            gameObjectOneWayCache = new GameObjectOneWayCache(container.transform, prefabsObjects);
        }

        protected override void OnIterationStart()
        {
            //Set new random points each newPointsIterations
            if (scenario.currentIteration % newPointsIteration == 0)
                points = PoissonDiscSampling.GeneratePoints(radius, regionSize, new Vector2(0, 0), rejectionSamples);

            if (points != null)
            {
                var pointsCopy = new List<Vector2>(points);
                int iterations = Mathf.Min(randomObjectsCount, points.Count());

                //Creates instances of prefabs on random positions from points
                for (int i = 0; i < iterations; i++)
                {
                    int pointId = Random.Range(0, pointsCopy.Count());

                    createObjAtRandPosAndRot(pointsCopy[pointId]);
                    pointsCopy.RemoveAt(pointId);
                }

                if (gameObjectOneWayCache.NumObjectsActive > 0)
                {
                    //Get random object from simObjects for targetObject
                    int objectId = Random.Range(0, simObjects.Count());
                    GameObject targetObject = simObjects[objectId];
                    Camera camera = Camera.main;

                    if (targetObject != null)
                    {
                        var targetPosition = targetObject.transform.position;
                        setRandPosition(camera, targetPosition);
                        setRandRotation(camera, targetPosition);
                        labelObjects(camera, targetObject);
                    }
                }
            }
        }

        protected override void OnIterationEnd()
        {
            gameObjectOneWayCache.ResetAllObjects();
            simObjects.Clear();
        }

        //Create instance of random selected prefab from prefabs and add it to simObjects
        private void createObjAtRandPosAndRot(Vector2 point)
        {
            float rotateY = Random.Range(0, 360f);

            var instance = gameObjectOneWayCache.GetOrInstantiate(prefabs.Sample());
            var labeling = instance.GetComponent<Labeling>();
            labeling.enabled = false;
            instance.transform.position = new Vector3(point.x, 0.1f, point.y);
            instance.transform.rotation = Quaternion.Euler(0, rotateY, 0);
            simObjects.Add(instance);
        }

        //Set random position (x, y, z) of camera with restriction of maxDistance to target object
        private void setRandPosition(Camera camera, Vector3 center)
        {
            maxDistance = Mathf.Max(height.max, maxDistance);
            float delta = height.max - height.min;
            float h = Random.Range(height.min, height.max);
            float r = Mathf.Sqrt(Mathf.Pow(maxDistance * h / delta, 2) - Mathf.Pow(h, 2));
            var randPoint = Random.insideUnitCircle * r;

            camera.transform.position = new Vector3(randPoint.x, h, randPoint.y) + center;
        }

        //Set random rotation of camera to target object
        private void setRandRotation(Camera camera, Vector3 center)
        {
            var cameraTransform = camera.transform;

            cameraTransform.LookAt(center);

            float c = camera.pixelHeight / 2 * Mathf.Sqrt(2);
            float x = Random.Range(0, c);
            float y = Random.Range(0, c);
            var cameraCenter = new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2);
            var point = new Vector2(x, y) + cameraCenter + Vector2.one * (-c / 2);
            var pointRotated = Vector2.zero;

            //Rotate point relative to center of camera screen
            //It makes that random area is a diamond inscribed in a rectangle (screen of the camera)
            //That's what I found sticking best to make rotation more random and keeping targetObject on view of the camera
            pointRotated.x = (float) (Mathf.Cos(45) * (point.x - cameraCenter.x)
                - Mathf.Sin(45) * (point.y - cameraCenter.y) + cameraCenter.x);
            pointRotated.y = (float) (Mathf.Sin(45) * (point.x - cameraCenter.x)
                                      + Mathf.Cos(45) * (point.y - cameraCenter.y) + cameraCenter.y);

            //Point to which camera is rotated
            var rotatePoint =
                camera.ScreenToWorldPoint(new Vector3(pointRotated.x, pointRotated.y, camera.farClipPlane));

            cameraTransform.LookAt(rotatePoint);

            //Checking if camera angle is not above and under the max and min rotation (from params)
            var cameraEulerAngles = cameraTransform.rotation.eulerAngles;
            float rotationX = cameraEulerAngles.x;

            rotationX = Mathf.Max(rotation.min, rotationX);
            rotationX = Mathf.Min(rotation.max, rotationX);
            cameraTransform.rotation = Quaternion.Euler(rotationX, cameraEulerAngles.y, cameraEulerAngles.z);
        }

        //Enable label for objects inside maxLabelingDistance
        private void labelObjects(Camera camera, GameObject targetObject)
        {
            for (int i = 0; i < simObjects.Count(); i++)
            {
                var gameObj = simObjects[i];
                var distance = Vector3.Distance(camera.transform.position, gameObj.transform.position);
                var angle = camera.transform.eulerAngles[0];
                var limit = maxLabelingDistance - maxLabelingDistance * Mathf.Abs(angle - 90) / 90;
                var distanceToTarget = Vector3.Distance(camera.transform.position, targetObject.transform.position);
                limit = Mathf.Max(distanceToTarget, limit);

                if (distance <= limit)
                {
                    var labeling = gameObj.GetComponent<Labeling>();

                    if (labeling != null)
                        labeling.enabled = true;
                }
            }
        }

        //Color the gameObject on color
        private void hintObject(Color color, GameObject gameObject)
        {
            if (gameObject != null)
            {
                var position = gameObject.transform.position;
                var material = gameObject.GetComponent<MeshRenderer>().material;
                var forward = position + Vector3.up * 2;

                material.color = color;
                Debug.DrawLine(position, forward, Color.green);
            }
        }
    }
}