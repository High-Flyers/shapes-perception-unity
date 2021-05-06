using System;
using System.Linq;
using Perception.RandomizerTags;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Rendering;

namespace Perception.Randomizers
{
    [Serializable]
    [AddRandomizerMenu("Perception/CustomRandomizers/MaterialRandomizer")]
    public class MaterialRandomizer : Randomizer
    {
        public MaterialParameter materials;
        public TextureParameter textures;

        protected override void OnIterationStart()
        {
            var materialsArray = this.materials.categories.Select((element) => element.Item1).ToArray();
            var materialId = UnityEngine.Random.Range(0, materialsArray.Count());
            var tags = tagManager.Query<MaterialRandomizerTag>();

            foreach (var tag in tags)
            {
                var material = materialsArray[materialId];
                var meshRenderer = tag.GetComponent<MeshRenderer>();
                
                material.mainTextureScale = new Vector2(40, 40);
                meshRenderer.material = material;
            }
        }
    }
}