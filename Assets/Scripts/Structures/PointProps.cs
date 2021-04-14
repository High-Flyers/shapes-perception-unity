namespace Structures
{
    public class PointProps
    {
        public float radius;
        public float prob;
        public int gridsToSearch;

        public PointProps(float[] props, float cellSize)
        {
            this.radius = props[0];
            this.prob = props[1];
            this.gridsToSearch = getGridsToSearch(cellSize);
        }

        private int getGridsToSearch(float cellSize)
            => (int) (radius / cellSize) + 1;
    }
}