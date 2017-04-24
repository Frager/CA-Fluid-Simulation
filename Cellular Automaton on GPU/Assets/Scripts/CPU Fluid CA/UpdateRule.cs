using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{
    

    public class UpdateRule : MonoBehaviour
    {
        public Element[] elements;

        [HideInInspector]
        public int maxVolume;

        public virtual void updateCells(Cell[,,] currentGen, Cell[,,] newGen) { }

        public virtual void UpdateGPUCells(GPUCell[,,] currentGen, GPUCell[,,] newGen) { }

        public virtual double MeanMilliseconds() { return 0;  }
    }
}
