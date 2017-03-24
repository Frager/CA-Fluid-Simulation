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
        public UpdateRule updateRule;

        private Texture3D texture3D;
        //stores update results for next generation
        private Cell[,,] newGen;
        //perform update with data of current generation
        private Cell[,,] currentGen;
        //private CPUFluidCA CA;
        private Element[] Elements;
        
        //init Cells (grid) and Element List
        void Start()
        {
            CPUFluidCA CA = new CPUFluidCA();
            newGen = CA.initGrid(gridSize, maxVolume, elementCount);
            currentGen = CA.initGrid(gridSize, maxVolume, elementCount);
            initElements();
            
            texture3D = new Texture3D(gridSize, gridSize, gridSize, TextureFormat.RGBA32, false);

            testMaterial.SetTexture("_MainTex", texture3D);

            visuals.GenerateVisuals(transform.position, gridSize, gridSize, gridSize, testMaterial);
            
        }

        // Update is called once per frame
        void Update()
        {
            updateRule.updateCells(currentGen, newGen);
            currentGen = newGen;
            updateTexture();
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
                        colors[x + y * gridSize + z * gridsize2] = (Color)currentGen[x,y,z];
                    }
                }
            }
            texture3D.SetPixels(colors);
            texture3D.Apply();
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