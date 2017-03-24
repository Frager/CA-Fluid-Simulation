using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{
    public class UpdateRule : MonoBehaviour
    {
        public virtual void updateCells(Cell[,,] currentGen, Cell[,,] newGen) { }
    }
}
