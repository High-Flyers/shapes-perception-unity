using System;
using System.Collections.Generic;
using System.Linq;
using Structures;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Randomizers.Utilities;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Perception.Randomizers
{
    [Serializable]
    [AddRandomizerMenu("Perception/CustomRandomizers/ObjectsInCameraViewRandomizer")]
    public class ObjectsPlacementRandomizer : Randomizer
    {
        public GameObjectParameter prefabs;
        public float randomObjectsCount = 0.3f;

        public float maxLabelingDistance = 100;
        public MinMaxParameter heightLimit = new MinMaxParameter(5, 40);
        public float rotationLimit = 60;

        private GameObject container;
        private GameObjectOneWayCache gameObjectOneWayCache;
        private List<GameObject> simObjects;
        private Camera camera;
        private GameObject background;
        private PointsInCameraViewGen pointsGen;

        protected override void OnAwake()
        {
            var prefabsObjects = prefabs.categories.Select((element) => element.Item1).ToArray();

            container = new GameObject("Foreground Objects");
            container.transform.parent = scenario.transform;
            gameObjectOneWayCache = new GameObjectOneWayCache(container.transform, prefabsObjects);
            camera = Camera.main;
            background = GameObject.Find("Background");
            var props = new List<float[]>();
            //props.Add(new[] {3f, 0.05f});
            props.Add(new[] {1.42f, 1f});
            pointsGen = new PointsInCameraViewGen(props, camera, maxLabelingDistance, 3);
        }

        protected override void OnIterationStart()
        {
            setRandPosition();
            setRandRotation();

            //Set new random points each newPointsIterations
            var points = pointsGen.generatePoints();
            Debug.Log(points.Count());

            if (points.Count() > 0)
            {
                int iterations;

                if (points.Count() > 2)
                    iterations = (int) Mathf.Round(points.Count() * randomObjectsCount);
                else
                    iterations = points.Count();
                
                iterations = Mathf.Max(iterations, 1);
                simObjects = new List<GameObject>(iterations);
                

                //Creates instances of prefabs on random positions from points
                for (int i = 0; i < iterations; i++)
                {
                    int pointId = Random.Range(0, points.Count());

                    createObjAtRandPosAndRot(points[pointId]);
                    points.RemoveAt(pointId);
                }

                if (gameObjectOneWayCache.NumObjectsActive > 0)
                    labelObjects();
            }
        }

        protected override void OnIterationEnd()
        {
            gameObjectOneWayCache.ResetAllObjects();
            simObjects.Clear();
        }
        
        

        //Create instance of random selected prefab from prefabs and add it to simObjects
        private void createObjAtRandPosAndRot(Point point)
        {
            float rotateY = Random.Range(0, 360f);
            
            var instance = gameObjectOneWayCache.GetOrInstantiate(prefabs.Sample());
            var labeling = instance.GetComponent<Labeling>();
            labeling.enabled = false;
            instance.transform.position = new Vector3(point.position.x, 0.1f, point.position.y);
            instance.transform.rotation = Quaternion.Euler(0, rotateY, 0);
            simObjects.Add(instance);
        }

        //Set random position (x, y, z) of camera with restriction of maxDistance to target object
        private void setRandPosition()
        {
            float h = Random.Range(heightLimit.min, heightLimit.max);

            camera.transform.position = new Vector3(0, h, 0);
        }

        private void setSizeOfBackground()
        {
            var distToPlane = camera.transform.position.y;
            var point = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distToPlane));
            var distance = Vector3.Distance(camera.transform.position, point);
            float height = 2.0f * Mathf.Tan(0.5f * camera.fieldOfView * Mathf.Deg2Rad) * distance;
            float width = height * Screen.width / Screen.height;
            Debug.Log(background.transform.localScale);
            Debug.DrawLine(camera.transform.position, point, Color.red);

            background.transform.localScale = new Vector3(width / 10, 1, height / 10);
            Debug.Log(point);
            background.transform.position = point;
        }

        //Set random rotation of camera to target object
        private void setRandRotation()
        {
            var rotationX = Random.Range(rotationLimit, 90);
            var cameraEulerAngles = camera.transform.rotation.eulerAngles;        
            
            camera.transform.rotation = Quaternion.Euler(rotationX, cameraEulerAngles.y, cameraEulerAngles.z);
        }

        //Enable label for objects inside maxLabelingDistance
        private void labelObjects()
        {
            for (int i = 0; i < simObjects.Count(); i++)
            {
                var gameObj = simObjects[i];
                var distance = Vector3.Distance(camera.transform.position, gameObj.transform.position);
                var angle = camera.transform.eulerAngles[0];
                var limit = maxLabelingDistance - maxLabelingDistance * Mathf.Abs(angle - 90) / 90;

                if (distance <= limit)
                {
                    var labeling = gameObj.GetComponent<Labeling>();

                    if (labeling != null)
                        labeling.enabled = true;
                }
            }
        }

        private float lawOfCosines(float a, float b, float angle)
            => Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2) - 2 * a * b * Mathf.Cos(angle));

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