using System;

namespace Structures
{
    [Serializable]
    public struct MinMaxParameter
    {
        public float min;
        public float max;

        public MinMaxParameter(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}