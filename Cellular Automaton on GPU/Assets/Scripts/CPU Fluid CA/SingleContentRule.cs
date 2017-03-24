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
                        if (y > 0 && currentGen[x, y - 1, z].getContent() == 0)
                        {
                            newGen[x, y, z].deleteContent();
                            didVerticalUpdate = true;
                        }
                        //if cell free and top cell has content copy content;
                        if (y < currentGen.GetLength(1) - 1 && currentGen[x, y, z].getContent() == 0 && currentGen[x, y + 1, z].getContent() != 0)
                        {
                            newGen[x, y, z].addContent(1);
                            didVerticalUpdate = true;
                        }
                        //Horizontal update
                        if (!didVerticalUpdate)
                        {
                            //if has content + direction towards empty cell, delete content and direction
                            if (currentGen[x, y, z].getContent() != 0 && CheckIfCopyContentToNeighbors(currentGen, x, y, z, currentGen[x, y, z].getDirection()))
                            {
                                newGen[x, y, z].deleteContent();
                                newGen[x, y, z].setDirection(Direction.none);
                            }
                            //if has no content + neighbor with content and direction towards current cell, copy content
                            if (currentGen[x, y, z].getContent() == 0 && CheckIfCopyContentfromNeighbors(currentGen, x, y, z))
                            {
                                newGen[x, y, z].addContent(1);
                            }
                            //setDirection flag
                            //if content has Content
                            if (currentGen[x, y, z].getContent() != 0)
                            {
                                //if bottom has direction and content set same Direction
                                Direction bottomDirection = getBottomDirection(currentGen, x, y, z);
                                if (bottomDirection != Direction.none)
                                {
                                    newGen[x, y, z].setDirection(bottomDirection);
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
            if (x < grid.GetLength(0) - 1 && grid[x + 1, y, z].getContent() == 0)
            {
                return Direction.xPos;
            }
            if (x > 0 && grid[x - 1, y, z].getContent() == 0)
            {
                return Direction.xNeg;
            }
            if (z < grid.GetLength(2) - 1 && grid[x, y, z + 1].getContent() == 0)
            {
                return Direction.zPos;
            }
            if (z > 0 && grid[x, y, z - 1].getContent() == 0)
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
            if (y > 0 && grid[x, y - 1, z].getContent() != 0)
            {
                return grid[x, y - 1, z].getDirection();
            }
            return Direction.none;
        }

        bool CheckIfCopyContentToNeighbors(Cell[,,] grid, int x, int y, int z, Direction dir)
        {
            if (x < grid.GetLength(0) - 1 && grid[x + 1, y, z].getContent() == 0 && dir == Direction.xPos)
            {
                return true;
            }
            if (x > 0 && grid[x - 1, y, z].getContent() == 0 && dir == Direction.xNeg)
            {
                return true;
            }
            if (z < grid.GetLength(2) - 1 && grid[x, y, z + 1].getContent() == 0 && dir == Direction.zPos)
            {
                return true;
            }
            if (z > 0 && grid[x, y, z - 1].getContent() == 0 && dir == Direction.zNeg)
            {
                return true;
            }
            return false;
        }

        bool CheckIfCopyContentfromNeighbors(Cell[,,] grid, int x, int y, int z)
        {
            if (x < grid.GetLength(0) - 1 && grid[x + 1, y, z].getDirection() == Direction.xNeg)
            {
                return true;
            }
            if (x > 0 && grid[x - 1, y, z].getDirection() == Direction.xPos)
            {
                return true;
            }
            if (z < grid.GetLength(2) - 1 && grid[x, y, z + 1].getDirection() == Direction.zNeg)
            {
                return true;
            }
            if (z > 0 && grid[x, y, z - 1].getDirection() == Direction.zPos)
            {
                return true;
            }
            return false;
        }
    }
}
