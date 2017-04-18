using System;

namespace CPUFluid
{
    public class PairRuleWithTemperature : UpdateRule
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
                        if (updateCycle % 2 == 0) //horizontal update
                        {
                            newGen[x, y, z].volume = 0;
                            newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume = 0;

                            //for each content (from highest to lowest density)
                            for (int id = currentGen[x, y, z].content.Length - 1; id >= 0; --id)
                            {
                                //mean = both content[id] / 2
                                mean = (currentGen[x, y, z].content[id] + currentGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].content[id]) / 2;
                                //the difference from current cell content to mean
                                //if difference > 0 current cell gets content from neighbour cell
                                //if difference < 0 neighbour cell gets content from current cell
                                difference = (mean - currentGen[x, y, z].content[id]);

                                //if one cell content == mean set difference = 0 
                                difference *= (1 - Convert.ToInt32(mean == currentGen[x, y, z].content[id] || mean == currentGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].content[id]));

                                //take viscosity into account (difference - viscosity)
                                amount = Math.Sign(difference) * Math.Max(Math.Abs(difference) - elements[id].viscosity, 0);

                                if ((newGen[x, y, z].volume + currentGen[x, y, z].content[id] + amount) > maxVolume)
                                {
                                    amount -= (newGen[x, y, z].volume + currentGen[x, y, z].content[id] + amount - maxVolume);
                                }

                                if ((newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume + currentGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].content[id] - amount) > maxVolume)
                                {
                                    amount += newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume + currentGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].content[id] - amount - maxVolume;
                                }

                                //mix temparature with content
                                if (amount > 0)
                                {
                                    //Sum of weighted Temperatures = SUM(temp * content amount of this temp)
                                    float sumWeightedTemps = newGen[x, y, z].temperature * newGen[x, y, z].volume + newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature * amount;
                                    //normalize Temp (Temp / total amount)
                                    newGen[x, y, z].temperature = sumWeightedTemps / (float)(newGen[x, y, z].volume +amount);
                                }
                                if (amount < 0)
                                {
                                    float sumWeightedTemps = newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature * newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume - newGen[x, y, z].temperature * amount;
                                    newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature = sumWeightedTemps / (float)(newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume - amount);
                                }

                                //swap contents
                                newGen[x, y, z].content[id] = currentGen[x, y, z].content[id] + amount;
                                newGen[x, y, z].volume += currentGen[x, y, z].content[id] + amount;
                                newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].content[id] = currentGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].content[id] - amount;
                                newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume += currentGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].content[id] - amount;
                            }
                        }
                        else //vertical update if (updateCycle % 2 == 1)
                        {
                            //sets volume of both cells to 0
                            newGen[x, y, z].volume = 0;
                            newGen[x, y + shift[updateCycle][1], z].volume = 0;
                            //for each content (from highest to lowest density)
                            for (int id = currentGen[x, y, z].content.Length - 1; id >= 0; --id)
                            {
                                //sum of bottom and top elenemt[id] amount
                                amount = (currentGen[x, y, z].content[id] + currentGen[x, y + shift[updateCycle][1], z].content[id]);
                                //min of available space in bottom cell or content amount
                                int bottom = (int)Math.Min(maxVolume - newGen[x, y, z].volume, Math.Min(elements[id].density, 1) * amount);

                                newGen[x, y, z].content[id] = bottom;
                                newGen[x, y, z].volume += bottom;

                                newGen[x, y + shift[updateCycle][1], z].content[id] = amount - bottom;
                                newGen[x, y + shift[updateCycle][1], z].volume += amount - bottom;
                            }
                        }
                    }
                }
            }
            updateCycle = (updateCycle + 1) % shift.Length;
        }
    }
}
