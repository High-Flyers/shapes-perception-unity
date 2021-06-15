using System;
using System.Linq;
using Perception.RandomizerTags;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using UnityTemplateProjects.Enum;
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
                        var renderer = tag.gameObject.GetComponent<Renderer>();

                        if (tag.type == ColorObjectType.Figure || tag.type == ColorObjectType.Stick )
                        {
                            renderer.material.SetColor(k_BaseColor, colorParameter.Sample());
                        }
                        else if(tag.type == ColorObjectType.Plane)
                        {
                            var copyColorParameter = new ColorHsvaParameter {saturation = new UniformSampler(0f, 0.25f), value = new UniformSampler(0.8f, 1f)};
                            renderer.material.color = copyColorParameter.Sample();
                        }
                        else if(tag.type == ColorObjectType.FigureOnPlane)
                        {
                            var copyColorParameter = new ColorHsvaParameter {saturation = new UniformSampler(0.35f, 1f)};
                            renderer.material.color = copyColorParameter.Sample();
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