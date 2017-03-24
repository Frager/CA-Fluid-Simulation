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
                        newGen[x, y, z] = currentGen[x, y, z].copyCell();
                        //if bottom cell empty delete content
                        if (y > 0 && currentGen[x, y - 1, z].getContent() == 0)
                        {
                            newGen[x, y, z].deleteContent();
                        }
                        //if top cell has content copy content;
                        if (y < currentGen.GetLength(1) - 1 && currentGen[x, y + 1, z].getContent() != 0)
                        {
                            newGen[x, y, z].addContent(1);
                        }



                        //if (y == currentGen.GetLength(1) - 1)
                        //{
                        //    newGen[x, y, z].addContent(1);
                        //}
                        //else {
                        //    newGen[x, y, z] = currentGen[x, y + 1, z];
                        //}
                    }
                }
            }
        }

        public override void UpdateGPUCells(GPUCell[,,] OldCells, GPUCell[,,] NewCells)
        {
            for (int z = 0; z < OldCells.GetLength(2); ++z)
            {
                for (int y = 0; y < OldCells.GetLength(1); ++y)
                {
                    for (int x = 0; x < OldCells.GetLength(0); ++x)
                    {
                        //Update Give
                        int content = OldCells[x,y,z].volume;

                        int pressure = 0;

                        if (y > 0 && content > 0 && OldCells[x,y-1,z].volume < 3)
                        {
                            pressure += 16;
                            --content;
                        }

                        if (x > 0 && content - OldCells[x-1,y,z].volume > 1)
                        {
                            pressure += 8;
                            --content;
                        }

                        if (x < OldCells.GetLength(0) - 1 && content - OldCells[x+1,y,z].volume > 1)
                        {
                            pressure += 4;
                            --content;
                        }

                        if (z > 0 && content - OldCells[x,y,y-1].volume > 1)
                        {
                            pressure += 2;
                            --content;
                        }

                        if (z < OldCells.GetLength(2) - 1 && content - OldCells[x,y,z+1].volume > 1)
                        {
                            pressure += 1;
                            --content;
                        }

                        if (y < OldCells.GetLength(1) - 1 && content - OldCells[x,y+1,z].volume > 3)
                        {
                            pressure += 32;
                            --content;
                        }

                        NewCells[x,y,z].volume = content;
                        NewCells[x, y, z].direction = pressure;
                    }
                }
            }
        }
    }
}
