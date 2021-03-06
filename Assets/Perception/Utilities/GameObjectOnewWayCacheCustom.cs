using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers.Utilities;

namespace Perception.Utilities
{
    public class GameObjectOneWayCacheCustom : GameObjectOneWayCache
    {
        public GameObjectOneWayCacheCustom(Transform parent, GameObject[] prefabs) : base(parent, prefabs)
        {
        }

        public new void ResetAllObjects()
        {
            base.ResetAllObjects();
        }
    }
}