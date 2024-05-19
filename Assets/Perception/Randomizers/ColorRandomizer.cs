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
                            var copyColorParameter = new ColorHsvaParameter {saturation = new UniformSampler(0f, 0.15f), value = new UniformSampler(0.85f, 1f)};
                            renderer.material.color = copyColorParameter.Sample();
                        }
                        else if(tag.type == ColorObjectType.FigureOnPlane)
                        {
                            var copyColorParameter = new ColorHsvaParameter {saturation = new UniformSampler(0.35f, 1f)};
                            renderer.material.color = copyColorParameter.Sample();
                        }
                        else if(tag.type == ColorObjectType.BlueBall)
                        {
                            var copyColorParameter = new ColorHsvaParameter { hue = new UniformSampler(0.583f, 0.69f), saturation = new UniformSampler(0.6f, 1.0f), value = new UniformSampler(0.5f, 0.9f)};
                            renderer.material.color = copyColorParameter.Sample();
                        }
                        else if(tag.type == ColorObjectType.RedBall)
                        {
                            var range1 = new UniformSampler(0f, 0.07f);
                            var range2 = new UniformSampler(0.96f, 1f);
                            var selectedSampler = range1; 
                            if (Random.Range(0f, 1f) > 0.5f)
                                selectedSampler = range2;

                            var copyColorParameter = new ColorHsvaParameter { hue = selectedSampler, saturation = new UniformSampler(0.75f, 1.0f), value = new UniformSampler(0.65f, 0.9f)};
                            renderer.material.color = copyColorParameter.Sample();
                        }
                        else if(tag.type == ColorObjectType.VioletBall)
                        {
                            var copyColorParameter = new ColorHsvaParameter { hue = new UniformSampler(0.73f, 0.88f), saturation = new UniformSampler(0.6f, 1.0f), value = new UniformSampler(0.45f, 0.8f)};
                            renderer.material.color = copyColorParameter.Sample();
                        }
                        else if(tag.type == ColorObjectType.YellowBall)
                        {
                            var copyColorParameter = new ColorHsvaParameter { hue = new UniformSampler(0.083f, 0.20f), saturation = new UniformSampler(0.6f, 1.0f), value = new UniformSampler(0.75f, 1f)};
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