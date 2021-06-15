using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityTemplateProjects.Enum;

namespace Perception.RandomizerTags
{
    [AddComponentMenu("Perception/CustomRandomizerTags/ColorRandomizerTag")]
    public class ColorRandomizerTag : RandomizerTag
    {
        public ColorObjectType type;
    }
}