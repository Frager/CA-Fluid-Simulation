using System;
using System.Diagnostics;

namespace CPUFluid
{
    public class PairRuleWithTemperature : UpdateRule
    {
        private int updateCycle = 0;

        private int[][] shift = { new int[] { 1, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 }, };

        private int[][] offset = { new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 } };
        private int mean, difference, amount, volume;

        private float temperatureSpread = 32f;

        private Stopwatch stopWatch = new Stopwatch();
        private TimeSpan ts;

        private double accumulatedMilliseconds = 0;
        private int counter = 0;

        public override void updateCells(Cell[,,] currentGen, Cell[,,] newGen)
        {
            stopWatch = Stopwatch.StartNew();
            for (int z = offset[updateCycle][2]; z < currentGen.GetLength(2) - offset[updateCycle][2]; z += (1 + shift[updateCycle][2]))
            {
                for (int y = offset[updateCycle][1]; y < currentGen.GetLength(1) - offset[updateCycle][1]; y += (1 + shift[updateCycle][1]))
                {
                    for (int x = offset[updateCycle][0]; x < currentGen.GetLength(0) - offset[updateCycle][0]; x += (1 + shift[updateCycle][0]))
                    {
                        //newGen[x, y, z] = currentGen[x, y, z];
                        //newGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]] = currentGen[x + shift[updateCycle][0], y + shift[updateCycle][1], z + shift[updateCycle][2]];

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

                                //temperature excange from new content
                                if (amount > 0)
                                {
                                    //Sum of weighted Temperatures = SUM(temp * content amount of this temp)
                                    float sumWeightedTemps = newGen[x, y, z].temperature * (currentGen[x, y, z].volume + 1) + currentGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature * amount;
                                    //normalize Temp (Temp / total amount)
                                    newGen[x, y, z].temperature = sumWeightedTemps / (float)(currentGen[x, y, z].volume + 1 + amount);
                                }
                                if (amount < 0)
                                {
                                    float sumWeightedTemps = newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature * (newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume + 1) - newGen[x, y, z].temperature * amount;
                                    newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature = sumWeightedTemps / (float)(newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume + 1 - amount);
                                }
                                else
                                {
                                    float difference = (newGen[x, y, z].temperature - newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature)/ temperatureSpread;
                                    newGen[x, y, z].temperature -= difference / ((float)newGen[x, y, z].volume + 1f);
                                    newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature += difference / ((float)newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume + 1f); ;
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
                            //newGen[x, y, z].volume = 0;
                            volume = 0;
                            //newGen[x, y + shift[updateCycle][1], z].volume = 0;
                            //for each content (from highest to lowest density)
                            for (int id = currentGen[x, y, z].content.Length - 1; id >= 0; --id)
                            {
                                //sum of bottom and top elenemt[id] amount
                                amount = (currentGen[x, y, z].content[id] + currentGen[x, y + shift[updateCycle][1], z].content[id]);
                                //min of available space in bottom cell or content amount
                                int bottom = (int)Math.Min(maxVolume - volume, Math.Min(elements[id].density, 1) * amount);

                                difference = bottom - newGen[x, y, z].content[id];
                                //temperature excange from new content to bottom cell
                                if (difference > 0)
                                {
                                    //Sum of weighted Temperatures = SUM(temp * content amount of this temp)
                                    float sumWeightedTemps = newGen[x, y, z].temperature * currentGen[x, y, z].volume + currentGen[x, y + shift[updateCycle][1], z].temperature * difference;
                                    //normalize Temp (Temp / total amount)
                                    newGen[x, y, z].temperature = sumWeightedTemps / (float)(currentGen[x, y, z].volume + difference);
                                }

                                newGen[x, y, z].content[id] += difference;
                                newGen[x, y, z].volume += difference;
                                volume += bottom;
                                
                                difference = amount - bottom - newGen[x, y + shift[updateCycle][1], z].content[id];
                                //temperature excange from new content to top cell
                                if (difference > 0)
                                {
                                    float sumWeightedTemps = newGen[x, y + shift[updateCycle][1], z].temperature * currentGen[x, y + shift[updateCycle][1], z].volume + currentGen[x, y , z].temperature * difference;
                                    newGen[x, y + shift[updateCycle][1], z].temperature = sumWeightedTemps / (float)(currentGen[x, y + shift[updateCycle][1], z].volume + difference);
                                }
                                    

                                newGen[x, y + shift[updateCycle][1], z].content[id] += difference;
                                newGen[x, y + shift[updateCycle][1], z].volume += difference;
                                float tempDifference = (newGen[x, y, z].temperature - newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature) / temperatureSpread;
                                    newGen[x, y, z].temperature += tempDifference / ((float)newGen[x, y, z].volume + 1f);
                                    newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].temperature -= tempDifference / ((float)newGen[x + shift[updateCycle][0], y, z + shift[updateCycle][2]].volume + 1f);
                            }


                            ////testing aggregate change
                            //if (y == 0) currentGen[x, y, z].temperature = 100;
                            //if (y == currentGen.GetLength(2) - 2) currentGen[x, y, z].temperature = 0;


                            //check for aggregate state changes
                            for (int id = elements.Length - 1; id >= 0; --id)
                            {
                                if (newGen[x, y, z].content[id] > 0)
                                {
                                    if (currentGen[x, y, z].temperature >= elements[id].evaporateTemperature)
                                    {
                                        //temperature cost to evaporate
                                        newGen[x, y, z].temperature -= 2f;
                                        --newGen[x, y, z].content[id];
                                        ++newGen[x, y, z].content[elements[id].evaporateElementId];
                                    }
                                    if (currentGen[x, y, z].temperature < elements[id].freezeTemperature)
                                    {
                                        //temperature cost for freezing
                                        newGen[x, y, z].temperature += 2f;
                                        --newGen[x, y, z].content[id];
                                        ++newGen[x, y, z].content[elements[id].freezeElementId];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            updateCycle = (updateCycle + 1) % shift.Length;
            stopWatch.Stop();
            accumulatedMilliseconds += stopWatch.Elapsed.TotalMilliseconds; 
            ++counter;
        }

        public override double MeanMilliseconds()
        {
            return accumulatedMilliseconds / counter;
        }
    }
}
