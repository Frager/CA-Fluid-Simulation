using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{
    public class CPUFluidManager : MonoBehaviour
    {
        public int gridSize = 16;
        public int maxVolume = 8;
        int elementCount = 3;

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

        public Representation visualsRepresentation = Representation.Content;

        public enum Representation
        {
            Content, Temperature
        }

        void Start()
        {
            //init Cells (grid) and Element List
            CPUFluidCA CA = new CPUFluidCA();
            newGen = CA.initGrid(gridSize, maxVolume, elementCount);
            currentGen = CA.initGrid(gridSize, maxVolume, elementCount);

            for(int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    currentGen[x, 0, z].temperature = 100;
                    currentGen[x, gridSize - 1, z].temperature = 0;
                }
            }

            initElements();
            updateRule.elements = elements;
            updateRule.maxVolume = maxVolume;
            texture3D = new Texture3D(gridSize, gridSize, gridSize, TextureFormat.RGBA32, false);

            testMaterial.SetTexture("_MainTex", texture3D);

            visuals.GenerateVisuals(transform.position, gridSize, gridSize, gridSize, testMaterial);

            updateTexture();
        }
        
        float timer = 0;
        float timeframe = 0.02f;
        int updateCount = 0;
        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                //for testing if content is correct
                if (updateCount == 200)
                {
                    int content = 0;
                    for (int x = 0; x < currentGen.GetLength(0); ++x)
                    {
                        for (int y = 0; y < currentGen.GetLength(1); ++y)
                        {
                            for (int z = 0; z < currentGen.GetLength(2); ++z)
                            {
                                content += currentGen[x, y, z].volume;
                            }
                        }
                    }
                    print("content " + content);
                }
                if (updateCount < 100)
                {
                }
                if (updateCount < 100)
                {
                    //currentGen[8, 15, 8].addContent(1, 0);
                    //currentGen[8, 8, 8].addContent(1, 2);
                }
                currentGen[8, 3, 8].addContent(1, 0);
                //currentGen[5, 14, 8].addContent(1, 1);
                //currentGen[8, 14, 8].addContent(1, 2);

                updateRule.updateCells(currentGen, newGen);
                CopyNewToCurrentCells();
                updateTexture();

                updateCount++;
                timer -= timeframe;
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
            if (Input.GetKey(KeyCode.C)) visualsRepresentation = Representation.Content;
            if (Input.GetKey(KeyCode.T)) visualsRepresentation = Representation.Temperature;

            int gridsize2 = gridSize * gridSize;
            Color[] colors = new Color[gridSize * gridsize2];
            for (int z = 0; z < gridSize; ++z)
            {
                for (int y = 0; y < gridSize; ++y)
                {
                    for (int x = 0; x < gridSize; ++x)
                    {
                        if(visualsRepresentation == Representation.Content)
                            colors[x + y * gridSize + z * gridsize2] = (Color)currentGen[x, y, z];
                        else
                            colors[x + y * gridSize + z * gridsize2] = currentGen[x, y, z].getTempColor();
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

            //Element (id, viscosity, density)

            //gasverhalten
            //elements[0] = new Element(0, -1, 0.45f);

            //wasserdampf
            elements[0] = new Element(0, 0, .3f);
            elements[0].setFreezeTransition(10f,2);
            //öl
            elements[1] = new Element(1, 2, 1f);
            //wasser
            elements[2] = new Element(2, 0, 2f);
            elements[2].setEvaporateTransition(100f, 0);
        }
    }
    
    public struct Element
    {
        int id;
        public int viscosity;
        public float density;
        public float evaporateTemperature;
        public float freezeTemperature;
        public int evaporateElementId;
        public int freezeElementId;

        public Element(int id, int viscosity, float density)
        {
            this.id = id;
            this.viscosity = viscosity;
            this.density = density;
            evaporateTemperature = float.MaxValue;
            evaporateElementId = id;
            freezeTemperature = float.MinValue;
            freezeElementId = id;
        }

        public void setFreezeTransition (float freezeTemp, int freezeElementId)
        {
            freezeTemperature = freezeTemp;
            this.freezeElementId = freezeElementId;
        }

        public void setEvaporateTransition(float evaporateTemp, int evaporateElementId)
        {
            evaporateTemperature = evaporateTemp;
            this.evaporateElementId = evaporateElementId;
        }
    }
}