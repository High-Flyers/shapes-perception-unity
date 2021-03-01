using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;

namespace Perception.Randomizers
{
    [Serializable]
    [AddRandomizerMenu("Perception/Randomizers/CameraRandomizer")]
    public class CameraRandomizer : Randomizer
    {
        private float rotationX = 0;
        private float rotationY = 0;
        private float positionY = 0;
        private Camera camera;

        protected override void OnIterationStart()
        {
            camera = Camera.main;
        }

        protected override void OnIterationEnd()
        {
            rotationX = UnityEngine.Random.Range(25, 90);
            rotationY = UnityEngine.Random.Range(0, 360);
            positionY = UnityEngine.Random.Range(5, 40);
            camera.transform.position = new Vector3(0, positionY, 0);
            camera.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        }
    }
}
