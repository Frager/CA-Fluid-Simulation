using System;

namespace CPUFluid
{
    public class PairRule : UpdateRule
    {
        private int updateCycle = 0;

        private int[][] shift = { new int[] { 1, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 }, };

        private int[][] offset = { new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 } };
        private int mean, difference, amount;

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

                        for (int id = 0; id < currentGen[x, y, z].content.Length; ++id)
                        {
                            if (updateCycle % 2 == 0)
                            {
                                mean = (currentGen[x, y, z].content[id] + currentGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]].content[id]) / 2;

                                difference = (mean - currentGen[x, y, z].content[id]);

                                if(Math.Abs(difference) < 2)
                                {
                                    difference = 0;
                                }

                                amount = Math.Sign(difference) * Math.Max(Math.Abs(difference) - 0 /*elements[id].viscosity*/, 0);

                                newGen[x, y, z].content[id] += amount;
                                newGen[x, y, z].volume += amount;
                                newGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]].content[id] -= amount;
                                newGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]].volume -= amount;

                                /*if (currentGen[x, y, z].content[id] > 0 || currentGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]].content[id] > 0)
                                {
                                    print("Update Cycle: " + updateCycle);
                                    print("Cell: (" + x + ", " + y + ", " + z + ") --- content: " + currentGen[x, y, z].content[id]);
                                    print("Neighbour: (" + (x + shift[updateCycle][0]) + ", " + (y + shift[updateCycle][1]) + ", " + (z + shift[updateCycle][2]) + ") --- content: " + currentGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]].content[id]);
                                    print("Mean: " + mean);
                                    print("Cell: (" + x + ", " + y + ", " + z + ") --- new content: " + newGen[x, y, z].content[id]);
                                    print("Neighbour: (" + (x + shift[updateCycle][0]) + ", " + (y + shift[updateCycle][1]) + ", " + (z + shift[updateCycle][2]) + ") --- new content: " + newGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]].content[id]);
                                    print("------------------------------------------------------------------");
                                }*/
                            }                      
                        }
                    }
                }
            }
            updateCycle = (updateCycle + 1) % shift.Length;
        }
    }
}
