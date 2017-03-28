using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{
    public class MultiContentWithFlagsRule : UpdateRule
    {
        private int updateCycle = 0;
        bool hasPassedDown;

        public override void updateCells(Cell[,,] currentGen, Cell[,,] newGen)
        {
            for (int z = 0; z < currentGen.GetLength(2); ++z)
            {
                for (int y = 0; y < currentGen.GetLength(1); ++y)
                {
                    for (int x = 0; x < currentGen.GetLength(0); ++x)
                    {
                        newGen[x, y, z] = currentGen[x, y, z].copyCell();
                        hasPassedDown = false;
                        int contentChange = 0;
                        //if volume > maxVolume push lightest content up
                        //if (currentGen[x, y, z].getVolume() > maxVolume)
                        //{
                        //    for (int id = 0; id < currentGen[x, y, z].content.Length; id++)
                        //    {
                        //        if (currentGen[x, y, z].content[id] > 0)
                        //        {
                        //            newGen[x, y, z].setDirection(Direction.yPos);
                        //            newGen[x, y, z].moveElementId = id;
                        //            break;
                        //        }
                        //    }
                        //}
                        //delete as much as bottom cell can take
                        if (y > 0 && currentGen[x, y - 1, z].getVolume() < maxVolume)
                        {
                            contentChange -= Mathf.Min(maxVolume - currentGen[x, y - 1, z].getVolume(), currentGen[x, y, z].getVolume());
                            hasPassedDown = true;
                        }
                        //take as much as possible from top cell
                        if (y < currentGen.GetLength(1) - 1 && currentGen[x, y, z].getVolume() < maxVolume && currentGen[x, y + 1, z].getVolume() != 0)
                        {
                            contentChange += Mathf.Min(maxVolume - currentGen[x, y, z].getVolume(), currentGen[x, y + 1, z].getVolume());
                        }
                        newGen[x, y, z].addContent(contentChange, 0);

                        //if has not passed down anything & volume > 1
                        if (!hasPassedDown && currentGen[x,y,z].getVolume() > 1)
                        {
                            //cycle through each content, lightest first
                            for (int id = 0; id < currentGen[x, y, z].content.Length; id++)
                            {
                                //if neighbor isLessContent(has less content && not full), set direction flag and moveElementId, then break
                                if (updateCycle == 0)
                                    if (x < currentGen.GetLength(0) - 1 && isLessContent(id, currentGen[x, y, z], currentGen[x + 1, y, z]))
                                    {
                                        newGen[x, y, z].setDirection(Direction.xPos);
                                        newGen[x, y, z].moveElementId = id;
                                        break;
                                    }
                                if (updateCycle == 1)
                                    if (x > 0 && isLessContent(id, currentGen[x, y, z], currentGen[x - 1, y, z]))
                                    {
                                        newGen[x, y, z].setDirection(Direction.xNeg);
                                        newGen[x, y, z].moveElementId = id;
                                        break;
                                    }
                                if (updateCycle == 2)
                                    if (z < currentGen.GetLength(2) - 1 && isLessContent(id, currentGen[x, y, z], currentGen[x, y, z + 1]))
                                    {
                                        newGen[x, y, z].setDirection(Direction.zPos);
                                        newGen[x, y, z].moveElementId = id;
                                        break;
                                    }
                                if (updateCycle == 3)
                                    if (z > 0 && isLessContent(id, currentGen[x, y, z], currentGen[x, y, z - 1]))
                                    {
                                        newGen[x, y, z].setDirection(Direction.zNeg);
                                        newGen[x, y, z].moveElementId = id;
                                        break;
                                    }
                            }
                        }
                        //if direction flag is set, set Direction Flag to none
                        if(currentGen[x, y, z].getDirection() != Direction.none)
                        {
                            newGen[x, y, z].setDirection(Direction.none);
                            newGen[x, y, z].moveElementId = -1;
                            //and delete content if not passed any content down
                            if (!hasPassedDown)
                            {
                                newGen[x, y, z].takeContent(1, currentGen[x, y, z].moveElementId);
                            }
                        }
                        //if neighbor has set direction flag towards current cell, take content
                        if (x > 0 && currentGen[x - 1, y, z].getDirection() == Direction.xPos)
                        {
                            if (currentGen[x - 1, y, z].moveElementId != 0) print("xPos");
                            newGen[x, y, z].addContent(1, currentGen[x - 1, y, z].moveElementId);
                        }
                        if (x < currentGen.GetLength(0) - 1 && currentGen[x + 1, y, z].getDirection() == Direction.xNeg)
                        {
                            if (currentGen[x + 1, y, z].moveElementId != 0) print("xNeg");
                            newGen[x, y, z].addContent(1, currentGen[x + 1, y, z].moveElementId);
                        }
                        if (z > 0 && currentGen[x, y, z - 1].getDirection() == Direction.zPos)
                        {
                            if (currentGen[x, y, z - 1].moveElementId != 0) print("zPos");
                            newGen[x, y, z].addContent(1, currentGen[x, y, z - 1].moveElementId);
                        }
                        if (z < currentGen.GetLength(2) - 1 && currentGen[x, y, z + 1].getDirection() == Direction.zNeg)
                        {
                            if (currentGen[x, y, z + 1].moveElementId != 0) print("zNeg");
                            newGen[x, y, z].addContent(1, currentGen[x, y, z + 1].moveElementId);
                        }
                        if (y > 0 && currentGen[x, y - 1, z].getDirection() == Direction.yPos)
                        {
                            if (currentGen[x, y - 1, z].moveElementId != 0) print("yPos");
                            newGen[x, y, z].addContent(1, currentGen[x, y - 1, z].moveElementId);
                        }
                    }
                }
            }
            updateCycle = (updateCycle + 1) % 4;
        }
        
        //return true if neighbor has less of element than current Cell and can take more content
        public bool isLessContent(int elementId, Cell currentCell, Cell neighbor)
        {
            if (currentCell.content[elementId]-1 > neighbor.content[elementId] && neighbor.volume < maxVolume) return true;
            else return false;
        }
    }
    
    //end namespace
}
