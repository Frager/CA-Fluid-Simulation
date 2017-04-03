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


                        //******************************************************************************
                        //swap 1 high with 1 low density content vertically
                        //******************************************************************************

                        //if content > 1 and bottom cell is full, add 1 lowest dense content from bottom and delete 1 of highest density
                        if (y > 0 && currentGen[x, y - 1, z].getVolume() == maxVolume && currentGen[x, y, z].getVolume() > 1)
                        {
                            int lightestIdBottom = currentGen[x, y - 1, z].getLightestContent();
                            int heaviestIdCurrent = currentGen[x, y, z].getHeaviestContent();
                            if(lightestIdBottom != heaviestIdCurrent)
                            {
                                newGen[x, y, z].addContent(1, lightestIdBottom);
                                newGen[x, y, z].takeContent(1, heaviestIdCurrent);
                            }
                        }
                        //if content full and top cell has content > 1 ,add 1 heighes dense content from top and delete 1 of lowest density
                        if (y < currentGen.GetLength(1) - 1 && currentGen[x, y, z].getVolume() == maxVolume && currentGen[x, y + 1, z].getVolume() > 1)
                        {
                            int lightestIdCurrent = currentGen[x, y, z].getLightestContent();
                            int heaviestIdTop = currentGen[x, y + 1, z].getHeaviestContent();
                            if (lightestIdCurrent != heaviestIdTop)
                            {
                                newGen[x, y, z].addContent(1, heaviestIdTop);
                                newGen[x, y, z].takeContent(1, lightestIdCurrent);
                            }
                        }

                        //******************************************************************************
                        //push content up if volume > maxVolume
                        //******************************************************************************

                        //if volume > maxVolume delete 1 content with lowest density
                        if (currentGen[x, y, z].getVolume() > maxVolume)
                        {
                            for (int id = 0; id < currentGen[x, y, z].content.Length; id++)
                            {
                                if (currentGen[x, y, z].content[id] > 0)
                                {
                                    newGen[x, y, z].takeContent(1, id);
                                    break;
                                }
                            }
                        }
                        //if bottom cell volume > maxVolume add 1 content from it with lowest density
                        if (y > 0 && currentGen[x, y - 1, z].getVolume() > maxVolume)
                        {
                            for (int id = 0; id < currentGen[x, y - 1, z].content.Length; id++)
                            {
                                if (currentGen[x, y - 1, z].content[id] > 0)
                                {
                                    newGen[x, y, z].addContent(1, id);
                                    break;
                                }
                            }
                        }

                        //******************************************************************************
                        //fill bottom cell up as much as possible
                        //******************************************************************************

                        //delete as much as bottom cell can take
                        if (y > 0 && currentGen[x, y - 1, z].getVolume() < maxVolume)
                        {
                            // contentChange is max amount that can be passed down to botom cell
                            contentChange = Mathf.Min(maxVolume - currentGen[x, y - 1, z].getVolume(), currentGen[x, y, z].getVolume());
                            //pass down (delete) contentChange amount of the heaviest contents
                            for (int id = currentGen[x, y, z].content.Length - 1; id >= 0; --id)
                            {
                                if (contentChange == 0) break;
                                int content = currentGen[x, y, z].content[id];
                                while (content > 0 && contentChange > 0)
                                {
                                    --content;
                                    --contentChange;
                                    newGen[x, y, z].takeContent(1, id);
                                }
                            }
                            hasPassedDown = true;
                        }
                        //take as much as possible from top cell
                        if (y < currentGen.GetLength(1) - 1 && currentGen[x, y, z].getVolume() < maxVolume && currentGen[x, y + 1, z].getVolume() != 0)
                        {
                            //contentChange is max amount that can be taken from top cell
                            contentChange = Mathf.Min(maxVolume - currentGen[x, y, z].getVolume(), currentGen[x, y + 1, z].getVolume());
                            //take contentChange amount from heaviest contents from top cell
                            for (int id = currentGen[x, y + 1, z].content.Length - 1; id >= 0; --id)
                            {
                                if (contentChange == 0) break;
                                int content = currentGen[x, y + 1, z].content[id];
                                while (content > 0 && contentChange > 0)
                                {
                                    --content;
                                    --contentChange;
                                    newGen[x, y, z].addContent(1, id);
                                }
                            }
                        }

                        //******************************************************************************
                        // if has not passed down horizontal update
                        //******************************************************************************

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
                            newGen[x, y, z].addContent(1, currentGen[x - 1, y, z].moveElementId);
                        }
                        if (x < currentGen.GetLength(0) - 1 && currentGen[x + 1, y, z].getDirection() == Direction.xNeg)
                        {
                            newGen[x, y, z].addContent(1, currentGen[x + 1, y, z].moveElementId);
                        }
                        if (z > 0 && currentGen[x, y, z - 1].getDirection() == Direction.zPos)
                        {
                            newGen[x, y, z].addContent(1, currentGen[x, y, z - 1].moveElementId);
                        }
                        if (z < currentGen.GetLength(2) - 1 && currentGen[x, y, z + 1].getDirection() == Direction.zNeg)
                        {
                            newGen[x, y, z].addContent(1, currentGen[x, y, z + 1].moveElementId);
                        }
                        if (y > 0 && currentGen[x, y - 1, z].getDirection() == Direction.yPos)
                        {
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
