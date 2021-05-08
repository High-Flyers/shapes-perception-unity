using System;
using System.Linq;
using Perception.RandomizerTags;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using Random = UnityEngine.Random;

namespace Perception.Randomizers
{
    [Serializable]
    [AddRandomizerMenu("Perception/CustomRandomizers/ColorRandomizer")]
    public class ColorRandomizer : Randomizer
    {
        static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");
        
        public ColorHsvaParameter colorParameter;

        protected override void OnIterationStart()
        {
            var tags = tagManager.Query<ColorRandomizerTag>();
            RenderSettings.ambientIntensity = Random.Range(2f, 4.5f);
            foreach (var tag in tags)
            {
                string label = tag.GetComponent<Labeling>().labels[0];
                
                if (label.Contains("OnPlane"))
                {
                    string shapeName = label.Replace("OnPlane", "");

                    for (int i = 0; i < tag.gameObject.transform.childCount; i++)
                    {
                        var child = tag.gameObject.transform.GetChild(i);
                        var renderer = child.gameObject.GetComponent<Renderer>();

                        if (child.name == shapeName)
                        {
                            var copyColorParameter = new ColorHsvaParameter {saturation = new UniformSampler(0.4f, 1f)};
                            renderer.material.color = copyColorParameter.Sample();
                            
                        }
                        else if(child.name == "Plane")
                        {
                            var copyColorParameter = new ColorHsvaParameter {saturation = new UniformSampler(0f, 0.3f)};
                            renderer.material.color = copyColorParameter.Sample();
                        }
                    }
                }
                else
                {
                    var rendererTag = tag.GetComponent<Renderer>();
                    rendererTag.material.SetColor(k_BaseColor, colorParameter.Sample());
                    Debug.Log(rendererTag.material.color);
                }
            }
        }
    }
}