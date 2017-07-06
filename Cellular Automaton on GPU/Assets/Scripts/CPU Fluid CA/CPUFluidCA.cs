namespace CPUFluid
{
    //Create
    public class CPUFluidCA
    {
        //private int elementCount = 1;
        //private int gridSize = 8;

        private Cell[,,] grid;

        /// <summary>
        /// Creates a three dimensional array of initialized cells. Use this for initialisation.
        /// </summary>
        /// <param name="gridSize"> The length of one grid side. Gridsize^3 equals total number of cells </param>
        /// <param name="maxVolume"> The amount of contents one cell can hold </param>
        /// <param name="elementCount"> The number of different elements being simulated </param>
        /// <returns> A threedimensional array of initialized cells</returns>
        public Cell[,,] initGrid(int gridSize, int maxVolume, int elementCount)
        {
            //this.gridSize = gridSize;
            //this.elementCount = elementCount;
            initCells(gridSize, maxVolume, elementCount);
            return grid;
        }

        /// <summary>
        /// Initializes each Cell of an three dimensional Array.
        /// </summary>
        /// <param name="gridSize"> The length of one grid side. Gridsize^3 equals total number of cells </param>
        /// <param name="maxVolume"> The amount of contents one cell can hold </param>
        /// <param name="elementCount"> The number of different elements being simulated </param>
        private void initCells(int size, int maxVolume ,int elementCount)
        {
            grid = new Cell[size, size, size];
            for (int z = 0; z < size; ++z)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int x = 0; x < size; ++x)
                    {
                        grid[x, y, z] = new Cell(elementCount, maxVolume);
                    }
                }
            }
        }
    }

}