using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CPUFluid
{
    public class SingleContentRule : UpdateRule
    {
        public override void updateCells(Cell[,,] currentGen, Cell[,,] newGen)
        {
            for(int z = 0; z < currentGen.GetLength(2); ++z)
            {
                for (int y = 0; y < currentGen.GetLength(1); ++y)
                {
                    for (int x = 0; x < currentGen.GetLength(0); ++x)
                    {
                        if (y == currentGen.GetLength(1) - 1)
                        {
                            newGen[x, y, z].addContent(1);
                        }
                        else {
                            newGen[x, y, z] = currentGen[x, y + 1, z];
                        }
                    }
                }
            }
        }
    }
}
