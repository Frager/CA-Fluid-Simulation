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

        public Material testMaterial;
        public SimpleVisuals visuals;

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

            texture3D = new RenderTexture(gridSize, gridSize, 1);
            texture3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture3D.volumeDepth = gridSize;
            texture3D.enableRandomWrite = true;
            texture3D.Create();

            testMaterial.SetTexture("_MainTex", texture3D);

            visuals.GenerateVisuals(transform.position, gridSize, gridSize, gridSize);
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

        void updateTexture()
        {
            int gridsize2 = gridSize * gridSize;
            Color[] colors = new Color[gridSize * gridsize2];
            for (int z = 0; z < gridSize; ++z)
            {
                for (int y = 0; y < gridSize; ++y)
                {
                    for (int x = 0; x < gridSize; ++x)
                    {
                        colors[x + y * gridSize + z * gridsize2] = (Color)grid[x,y,z];
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