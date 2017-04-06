namespace CPUFluid
{
    public class PairRule : UpdateRule
    {
        private int updateCycle = 0;

        //TODO: Die int-Wert kann man auch so wie sie unten im Switch-Case vorkommen ein ein Array (8x6) packen und das Switch-Case-Statement eliminieren.
        //Shows in which direction the next update goes.
        private int shiftX, shiftY, shiftZ;

        private int offsetX, offsetY, offsetZ;

        public override void updateCells(Cell[,,] currentGen, Cell[,,] newGen)
        {
            for (int z = offsetZ; z < currentGen.GetLength(2) - offsetZ; z += (1 + shiftZ))
            {
                for (int y = offsetY; y < currentGen.GetLength(1) - offsetY; y += (1 + shiftY))
                {
                    for (int x = offsetX; x < currentGen.GetLength(0) - offsetX; x += (1 + shiftX))
                    {
                        newGen[x, y, z] = currentGen[x, y, z].copyCell();
                        newGen[x + shiftX, y + shiftY, z + shiftZ] = currentGen[x + shiftX, y + shiftY, z + shiftZ].copyCell();

                        for (int id = 0; id < currentGen[x, y, z].content.Length; id++)
                        {
                            if (updateCycle % 2 == 0)
                            {
                                int give = (int)(((float)(currentGen[x, y, z].content[id] - currentGen[x + shiftX, y + shiftY, z + shiftZ].content[id])) / (2f * elements[id].viscosity));

                                newGen[x, y, z].content[id] += ??;
                                newGen[x + shiftX, y + shiftY, z + shiftZ].content[id] -= ??;
                            }                      
                        }
                    }
                }
            }
            updateCycle = (updateCycle + 1) % 8;

            switch(updateCycle)
            {
                case 0:
                    shiftX = 1;
                    shiftY = 0;
                    shiftZ = 0;
                    offsetX = 0;
                    offsetY = 0;
                    offsetZ = 0;
                    break;
                case 2:
                    shiftX = 0;
                    shiftY = 0;
                    shiftZ = 1;
                    offsetX = 0;
                    offsetY = 0;
                    offsetZ = 0;
                    break;
                case 4:
                    shiftX = 1;
                    shiftY = 0;
                    shiftZ = 0;
                    offsetX = 1;
                    offsetY = 0;
                    offsetZ = 0;
                    break;
                case 6:
                    shiftX = 0;
                    shiftY = 0;
                    shiftZ = 1;
                    offsetX = 0;
                    offsetY = 0;
                    offsetZ = 1;
                    break;
            }
        }
    }
}
