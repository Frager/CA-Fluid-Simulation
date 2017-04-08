using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{
    public class CPUFluidManager : MonoBehaviour
    {
        public int gridSize = 16;
        public int maxVolume = 8;
        int elementCount = 2;

        public Material testMaterial;
        public SimpleVisuals visuals;
        public UpdateRule updateRule;

        private Texture3D texture3D;
        //stores update results for next generation
        private Cell[,,] newGen;
        //perform update with data of current generation
        private Cell[,,] currentGen;
        //private CPUFluidCA CA;
        private Element[] elements;
        
        void Start()
        {
            //init Cells (grid) and Element List
            CPUFluidCA CA = new CPUFluidCA();
            newGen = CA.initGrid(gridSize, maxVolume, elementCount);
            currentGen = CA.initGrid(gridSize, maxVolume, elementCount);
            initElements();
            updateRule.elements = elements;
            updateRule.maxVolume = maxVolume;
            texture3D = new Texture3D(gridSize, gridSize, gridSize, TextureFormat.RGBA32, false);

            testMaterial.SetTexture("_MainTex", texture3D);

            visuals.GenerateVisuals(transform.position, gridSize, gridSize, gridSize, testMaterial);

            updateTexture();
        }
        
        float timer = 0;
        float timeframe = 0.1f;
        float updateCount = 0;
        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                //for testing if content is correct
                if (updateCount == 300)
                {
                    int content = 0;
                    for (int x = 0; x < currentGen.GetLength(0) - 1; ++x)
                    {
                        for (int y = 0; y < currentGen.GetLength(1) - 1; ++y)
                        {
                            for (int z = 0; z < currentGen.GetLength(2) - 1; ++z)
                            {
                                content += currentGen[x, y, z].volume;
                            }
                        }
                    }
                    print("content " + content);
                }
                if (updateCount < 200)
                {
                        currentGen[8, 8, 8].addContent(1, 1);
                    //currentGen[7, 0, 7].addContent(1, 0);
                }
                //if (updateCount < 200)
                //{
                //        currentGen[8, 0, 8].addContent(1, 0);
                //    //currentGen[7, 0, 7].addContent(1, 1);
                //}
                updateCount++;
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
            elements = new Element[elementCount];

            for (int i = 0; i < elementCount; ++i)
            {
                elements[i] = new Element(i,  i + 1, (float)i + 0.5f);
            }
        }
    }
    
    public struct Element
    {
        int id;
        public int viscosity;
        public float density;

        public Element(int id, int viscosity, float density)
        {
            this.id = id;
            this.viscosity = viscosity;
            this.density = density;
        }
    }
}