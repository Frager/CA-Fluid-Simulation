using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{
    public class CPUFluidCA
    {

        private int elementCount = 1;
        private int maxVolume;
        private int gridSize = 8;

        private Cell[,,] grid;

        // Use this for initialization
        public Cell[,,] initGrid(int gridSize, int maxVolume, int elementCount)
        {
            this.gridSize = gridSize;
            this.maxVolume = maxVolume;
            this.elementCount = elementCount;
            initCells(gridSize);
            return grid;
        }

        

        private void initCells(int size)
        {
            grid = new Cell[size, size, size];
            for (int z = 0; z < size; ++z)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int x = 0; x < size; ++x)
                    {
                        grid[x, y, z] = new Cell(new Vector3(x, y, z), elementCount);
                    }
                }
            }
        }
    }

    public struct Element
    {
        int id;
        float viscosity;
        float density;

        public Element(int id, float viscosity, float density)
        {
            this.id = id;
            this.viscosity = viscosity;
            this.density = density;
        }
    }
    
    public struct Cell
    {
        Vector3 position;
        int[] content;
        int volume;

        public Cell(Vector3 position, int elementCount)
        {
            this.position = position;
            content = new int[elementCount];
            this.volume = 0;
        }

        public void addContent(int amount)
        {
            volume += amount;
        }

        static public implicit operator Color(Cell cell)
        {
            float blue = cell.volume;
            if (blue >= 1)
            {
                return new Color(0, 0, 1f, 1f);
            }
            return new Color(1f ,1f , 1f, 0.1f);
        }

    }
}