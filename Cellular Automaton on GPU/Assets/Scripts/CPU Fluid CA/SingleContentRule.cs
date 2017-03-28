using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CPUFluid
{
    public class SingleContentRule : UpdateRule
    {
        bool didVerticalUpdate;
        public override void updateCells(Cell[,,] currentGen, Cell[,,] newGen)
        {
            for(int z = 0; z < currentGen.GetLength(2); ++z)
            {
                for (int y = 0; y < currentGen.GetLength(1); ++y)
                {
                    for (int x = 0; x < currentGen.GetLength(0); ++x)
                    {
                        newGen[x, y, z] = currentGen[x, y, z].copyCell();
                        didVerticalUpdate = false;
                        //if bottom cell empty delete content
                        if (y > 0 && currentGen[x, y - 1, z].getVolume() == 0)
                        {
                            newGen[x, y, z].deleteContent();
                            didVerticalUpdate = true;
                        }
                        //if cell free and top cell has content copy content;
                        if (y < currentGen.GetLength(1) - 1 && currentGen[x, y, z].getVolume() == 0 && currentGen[x, y + 1, z].getVolume() != 0)
                        {
                            newGen[x, y, z].addContent(1);
                            didVerticalUpdate = true;
                        }
                        //Horizontal update
                        if (!didVerticalUpdate)
                        {
                            //if has content + direction towards empty cell, delete content and direction
                            if (currentGen[x, y, z].getVolume() != 0 && CheckIfCopyContentToNeighbors(currentGen, x, y, z, currentGen[x, y, z].getDirection()))
                            {
                                newGen[x, y, z].deleteContent();
                                newGen[x, y, z].setDirection(Direction.none);
                                continue;
                            }
                            //if has no content + neighbor with content and direction towards current cell, copy content
                            if (currentGen[x, y, z].getVolume() == 0 && CheckIfCopyContentfromNeighbors(currentGen, x, y, z))
                            {
                                newGen[x, y, z].addContent(1);
                            }
                            //setDirection flag
                            //if has Content
                            if (currentGen[x, y, z].getVolume() != 0)
                            {
                                //if bottom has direction and content set same Direction
                                Direction bottomDirection = getBottomDirection(currentGen, x, y, z);
                                if (bottomDirection != Direction.none)
                                {
                                    newGen[x, y, z].setDirection(bottomDirection);
                                }
                                Direction topDirection = getTopDirection(currentGen, x, y, z);
                                if (topDirection != Direction.none)
                                {
                                    newGen[x, y, z].setDirection(Direction.none);
                                }
                                else
                                {
                                    //else set direction to empty neighbors
                                    newGen[x, y, z].setDirection(ToEmptyNeighbor(currentGen, x, y, z));
                                    if (newGen[x, y, z].getDirection() == Direction.none)
                                    {
                                        newGen[x, y, z].setDirection(ToNeighborWithDirection(currentGen, x, y, z));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        //returns Direction to empty neighbors
        Direction ToEmptyNeighbor(Cell[,,] grid, int x, int y, int z)
        {
            if (x < grid.GetLength(0) - 1 && grid[x + 1, y, z].getVolume() == 0)
            {
                return Direction.xPos;
            }
            if (x > 0 && grid[x - 1, y, z].getVolume() == 0)
            {
                return Direction.xNeg;
            }
            if (z < grid.GetLength(2) - 1 && grid[x, y, z + 1].getVolume() == 0)
            {
                return Direction.zPos;
            }
            if (z > 0 && grid[x, y, z - 1].getVolume() == 0)
            {
                return Direction.zNeg;
            }
            return Direction.none;
        }

        //returns to neighbors with Direction
        Direction ToNeighborWithDirection(Cell[,,] grid, int x, int y, int z)
        {
            if (x < grid.GetLength(0) - 1 && grid[x + 1, y, z].getDirection() != Direction.none)
            {
                return Direction.xPos;
            }
            if (x > 0 && grid[x - 1, y, z].getDirection() != Direction.none)
            {
                return Direction.xNeg;
            }
            if (z < grid.GetLength(2) - 1 && grid[x, y, z + 1].getDirection() != Direction.none)
            {
                return Direction.zPos;
            }
            if (z > 0 && grid[x, y, z - 1].getDirection() != Direction.none)
            {
                return Direction.zNeg;
            }
            return Direction.none;
        }

        Direction getBottomDirection(Cell[,,] grid, int x, int y, int z)
        {
            if (y > 0 && grid[x, y - 1, z].getVolume() != 0)
            {
                return grid[x, y - 1, z].getDirection();
            }
            return Direction.none;
        }

        Direction getTopDirection(Cell[,,] grid, int x, int y, int z)
        {
            if (y < grid.GetLength(1) - 1)
            {
                return grid[x, y + 1, z].getDirection();
            }
            return Direction.none;
        }

        bool CheckIfCopyContentToNeighbors(Cell[,,] grid, int x, int y, int z, Direction dir)
        {
            if (x < grid.GetLength(0) - 1 && grid[x + 1, y, z].getVolume() == 0 && dir == Direction.xPos)
            {
                return true;
            }
            if (x > 0 && grid[x - 1, y, z].getVolume() == 0 && dir == Direction.xNeg)
            {
                return true;
            }
            if (z < grid.GetLength(2) - 1 && grid[x, y, z + 1].getVolume() == 0 && dir == Direction.zPos)
            {
                return true;
            }
            if (z > 0 && grid[x, y, z - 1].getVolume() == 0 && dir == Direction.zNeg)
            {
                return true;
            }
            return false;
        }

        bool CheckIfCopyContentfromNeighbors(Cell[,,] grid, int x, int y, int z)
        {
            if (x < grid.GetLength(0) - 1 && grid[x + 1, y, z].getDirection() == Direction.xNeg && grid[x + 1, y, z].getVolume() != 0)
            {
                return true;
            }
            if (x > 0 && grid[x - 1, y, z].getDirection() == Direction.xPos && grid[x - 1, y, z].getVolume() != 0)
            {
                return true;
            }
            if (z < grid.GetLength(2) - 1 && grid[x, y, z + 1].getDirection() == Direction.zNeg && grid[x, y, z + 1].getVolume() != 0)
            {
                return true;
            }
            if (z > 0 && grid[x, y, z - 1].getDirection() == Direction.zPos && grid[x, y, z - 1].getVolume() != 0)
            {
                return true;
            }
            return false;
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
