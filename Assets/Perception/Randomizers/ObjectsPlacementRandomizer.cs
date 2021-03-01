using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;

namespace Perception.Randomizers
{
    [Serializable]
    [AddRandomizerMenu("Perception/Randomizers/ObjectsInCameraViewRandomizer")]
    public class ObjectsPlacementRandomizer : Randomizer
    {
        public GameObjectParameter prefabs;
        public int randomObjectsCount = 20;
        
        public float radius = 1;
        public Vector2 regionSize = Vector2.one;
        public int rejectionSamples = 30;
        
        private List<GameObject> objects = new List<GameObject>();
        private List<Vector2> points;

        protected override void OnIterationStart()
        {
            // var camera = Camera.main;
            // var prefabObjects = prefabs.categories.Select((element) => element.Item1).ToArray();
            //
            // for(int i = 0; i < randomObjectsCount; i++)
            // {
            //     RaycastHit hit;
            //     float x = UnityEngine.Random.Range(0, camera.pixelWidth);
            //     float y = UnityEngine.Random.Range(0, camera.pixelHeight);
            //     var pos = new Vector3(x, y, 0);
            //     var ray = camera.ScreenPointToRay(pos);
            //
            //     if(Physics.Raycast(ray, out hit))
            //     {
            //         float rotateY = UnityEngine.Random.Range(0, 360f);
            //         int id = UnityEngine.Random.Range(0, prefabObjects.Length);
            //         
            //         Debug.DrawRay(ray.origin, ray.direction * 10, Color.green);
            //         objects.Add(GameObject.Instantiate(prefabObjects[id], hit.point, Quaternion.identity));
            //         objects.Last().transform.rotation = Quaternion.Euler(0, rotateY, 0);
            //     }
            // }
            points = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);

            if (points != null)
            {
                var prefabObjects = prefabs.categories.Select((element) => element.Item1).ToArray();
                
                foreach (var point in points)
                {
                    float rotateY = UnityEngine.Random.Range(0, 360f);
                    int id = UnityEngine.Random.Range(0, prefabObjects.Length);
                    var position = new Vector3(point.x, 0, point.y);
                    
                    objects.Add(GameObject.Instantiate(prefabObjects[id], position, Quaternion.identity));
                    objects.Last().transform.rotation = Quaternion.Euler(0, rotateY, 0);
                }
            }
        }

        protected override void OnIterationEnd()
        {
            for(int i = 0; i < objects.Count; i++)
                GameObject.Destroy(objects[i]);

            objects.Clear();
        }
    }
}
