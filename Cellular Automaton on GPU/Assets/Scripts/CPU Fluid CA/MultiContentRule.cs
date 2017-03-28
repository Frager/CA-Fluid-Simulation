using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CPUFluid
{
    public class MultiContentRule : UpdateRule
    {
        private int maxContent = 4;
        private int minContentToPass = 3;
        private int updateCycle = 0;
        bool didVerticalUpdate;

        public override void updateCells(Cell[,,] currentGen, Cell[,,] newGen)
        {
            for (int z = 0; z < currentGen.GetLength(2); ++z)
            {
                for (int y = 0; y < currentGen.GetLength(1); ++y)
                {
                    for (int x = 0; x < currentGen.GetLength(0); ++x)
                    {
                        newGen[x, y, z] = currentGen[x, y, z].copyCell();
                        didVerticalUpdate = false;
                        int contentChange = 0;
                        //delete as much as bottom cell can take
                        if (y > 0 && currentGen[x, y - 1, z].getVolume() < maxContent)
                        {
                            contentChange -= Mathf.Min(maxContent - currentGen[x, y - 1, z].getVolume(), currentGen[x, y, z].getVolume());
                            didVerticalUpdate = true;
                        }
                        //take as much as possible from top cell
                        if (y < currentGen.GetLength(1) - 1 && currentGen[x, y, z].getVolume() < maxContent && currentGen[x, y + 1, z].getVolume() != 0)
                        {
                            contentChange += Mathf.Min(maxContent - currentGen[x, y, z].getVolume(), currentGen[x, y + 1, z].getVolume());
                            didVerticalUpdate = true;
                        }
                        newGen[x, y, z].addContent(contentChange);
                        //Horizontal update
                        //if (contentChange == 0)
                        int content = currentGen[x, y, z].getVolume();
                        if (!didVerticalUpdate || content >= minContentToPass)
                        {

                            if (updateCycle == 0)
                                if (x < currentGen.GetLength(0) - 1)
                                {
                                    if (currentGen[x + 1, y, z].getVolume() >= minContentToPass && currentGen[x + 1, y, z].getVolume() > content && content < maxContent - 1)
                                        ++contentChange;
                                    if (content >= minContentToPass && currentGen[x + 1, y, z].getVolume() < content && currentGen[x + 1, y, z].getVolume() < maxContent)
                                        --contentChange;
                                }
                            if (updateCycle == 0)
                                if (x > 0)
                                {
                                    if (content >= minContentToPass && content > currentGen[x - 1, y, z].getVolume() && currentGen[x - 1, y, z].getVolume() < maxContent - 1)
                                        --contentChange;
                                    if (currentGen[x - 1, y, z].getVolume() >= minContentToPass && content < currentGen[x - 1, y, z].getVolume() && content < maxContent)
                                        ++contentChange;
                                }
                            if (updateCycle == 1)
                                if (z < currentGen.GetLength(2) - 1)
                                {
                                    if (currentGen[x, y, z + 1].getVolume() >= minContentToPass && currentGen[x, y, z + 1].getVolume() > content && content < maxContent - 1)
                                        ++contentChange;
                                    if (content >= minContentToPass && currentGen[x, y, z + 1].getVolume() < content && currentGen[x, y, z + 1].getVolume() < maxContent)
                                        --contentChange;
                                }
                            if (updateCycle == 1)
                                if (z > 0)
                                {
                                    if (content >= minContentToPass &&  content > currentGen[x, y, z - 1].getVolume() && currentGen[x, y, z - 1].getVolume() < maxContent - 1)
                                        --contentChange;
                                    if (currentGen[x, y, z - 1].getVolume() >= minContentToPass &&  content < currentGen[x, y, z - 1].getVolume() && content < maxContent)
                                        ++contentChange;
                                }
                            newGen[x, y, z].addContent(contentChange);
                        }
                    }
                }
            }
            updateCycle = (updateCycle + 1) % 2;
        }
    }
}