using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{
    public class CPUFluidManager : MonoBehaviour
    {

        int gridSize = 8;
        int maxVolume = 8;
        int elementCount = 1;

        private RenderTexture texture3D;
        private Cell[,,] grid;
        private CPUFluidCA CA;
        private Element[] Elements;

        //init Cells (grid) and Element List
        void Start()
        {
            CA = new CPUFluidCA();
            grid = CA.initGrid(gridSize, maxVolume, elementCount);
            initElements();
        }

        // Update is called once per frame
        void Update()
        {
            for (int z = 0; z < gridSize; ++z)
            {
                for (int y = 0; y < gridSize; ++y)
                {
                    for (int x = 0; x < gridSize; ++x)
                    {
                        //TODO:
                        //Update Cells
                    }
                }
            }
        }

        //TODO:
        //replace placeholder elements with real elements
        private void initElements()
        {
            Elements = new Element[elementCount];

            for (int i = 0; i < elementCount; ++i)
            {
                Elements[i] = new Element(i, i, i);
            }
        }
    }
}