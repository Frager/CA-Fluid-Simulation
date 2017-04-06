using System;

namespace CPUFluid
{
    public class PairRule : UpdateRule
    {
        private int updateCycle = 0;

        private int[][] shift = { new int[] { 1, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 } };

        private int[][] offset = { new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 } };

        private int mean, difference1, difference2;

        public override void updateCells(Cell[,,] currentGen, Cell[,,] newGen)
        {
            for (int z = offset[updateCycle][2]; z < currentGen.GetLength(2) - offset[updateCycle][2]; z += (1 + shift[updateCycle][2]))
            {
                for (int y = offset[updateCycle][1]; y < currentGen.GetLength(1) - offset[updateCycle][1]; y += (1 + shift[updateCycle][1]))
                {
                    for (int x = offset[updateCycle][0]; x < currentGen.GetLength(0) - offset[updateCycle][0]; x += (1 + shift[updateCycle][0]))
                    {
                        newGen[x, y, z] = currentGen[x, y, z].copyCell();
                        newGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]] = currentGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]].copyCell();

                        for (int id = 0; id < currentGen[x, y, z].content.Length; id++)
                        {
                            if (updateCycle % 2 == 0)
                            {
                                mean = (currentGen[x, y, z].content[id] - currentGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]].content[id]) / 2;

                                newGen[x, y, z].content[id] += (mean - Math.Sign(mean) * elements[id].viscosity);
                                newGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]].content[id] += (mean - Math.Sign(mean) * elements[id].viscosity);
                            }                      
                        }
                    }
                }
            }
            updateCycle = (updateCycle + 1) % 8;
        }
    }
}
