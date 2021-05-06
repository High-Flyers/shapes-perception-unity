using System.Collections.Generic;

namespace Structures
{
    public class PointProps
    {
        public float radius;
        public float prob;
        public int gridsToSearch;

        public PointProps(float[] props, float cellSize, float maxRadius)
        {
            this.radius = props[0];
            this.prob = props[1];
            this.gridsToSearch = getGridsToSearch(cellSize, maxRadius);
        }

        private int getGridsToSearch(float cellSize, float maxRadius)
            => (int) ((radius + maxRadius) / cellSize) + 1;
    }
}