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
        
        void Start()
        {
            //init Cells (grid) and Element List
            CPUFluidCA CA = new CPUFluidCA();
            newGen = CA.initGrid(gridSize, maxVolume, elementCount);
            currentGen = CA.initGrid(gridSize, maxVolume, elementCount);
            initElements();
            
            texture3D = new Texture3D(gridSize, gridSize, gridSize, TextureFormat.RGBA32, false);

            testMaterial.SetTexture("_MainTex", texture3D);

            visuals.GenerateVisuals(transform.position, gridSize, gridSize, gridSize, testMaterial);

            updateTexture();
        }
        
        float timer = 0;
        float timeframe = 0.1f;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                //for testing
                currentGen[0, 7, 0].addContent(1);

                timer -= timeframe;
                updateRule.updateCells(currentGen, newGen);
                CopyNewToCurrentCells();
                updateTexture();
            }
        }

        void CopyNewToCurrentCells()
        {
            for (int z = 0; z < gridSize; ++z)
            {
                for (int y = 0; y < gridSize; ++y)
                {
                    for (int x = 0; x < gridSize; ++x)
                    {
                        currentGen[x, y, z] = newGen[x, y, z].copyCell();
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