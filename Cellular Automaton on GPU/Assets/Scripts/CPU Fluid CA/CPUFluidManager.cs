using UnityEngine;

namespace CPUFluid
{
    public class CPUFluidManager : MonoBehaviour
    {
        //gridsize^3 is the amount of cells in the 3 dimensional grid
        public int gridSize = 16;
        //the maximum amount of contents a cell can hold
        public int maxVolume = 8;
        //the number of elements that will be simulated in the cellular automata
        int elementCount = 3;

        public Material testMaterial;
        public SimpleVisuals visuals;
        //the rules performed each cellular automata update
        public UpdateRule updateRule;

        //visuals use this texture
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
            
            initElements();
            updateRule.elements = elements;
            updateRule.maxVolume = maxVolume;
            texture3D = new Texture3D(gridSize, gridSize, gridSize, TextureFormat.RGBA32, false);

            testMaterial.SetTexture("_MainTex", texture3D);

            visuals.GenerateVisuals(transform.position, gridSize, gridSize, gridSize, testMaterial);

            updateTexture();
        }
        
        float timer = 0;
        //how many ms to wait until next update will be performed
        float timeframe = 0.1f;
        //counts the number of performed updates
        int updateCount = 0;

        // the general unity monobehavior update
        // uses a timer to control simulation speed (one update every timeframe)
        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                
                if (updateCount < 1000)
                {
                    fillCell(8, 10, 8, 2, 1, 20);
                    fillCell(3, 10, 3, 1, 1, 1000);
                }

                updateGrid();

                updateCount++;
                timer -= timeframe;
            }
        }

        //fills a cell with content
        public void fillCell(int x, int y, int z, int elementId, int elementCount, int cellTemperature)
        {
            currentGen[x, y, z].temperature = cellTemperature;
            currentGen[x, y, z].addContent(elementCount, elementId);
        }

        //performes an update on all cells and updates the texture3D for visualisation 
        private void updateGrid()
        {
            updateRule.updateCells(currentGen, newGen);
            CopyNewToCurrentCells();
            updateTexture();
        }

        //copies the array of new generation of cells to the current aray of cells
        //needs to be performed between each update
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

        //updates the Texture to represent the current generation of cells
        //possible to switch between visual representations (Content and Temperature)
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


        
        /// <summary>
        /// initialises the element list. Edit element properties here
        /// </summary>
        private void initElements()
        {
            elements = new Element[elementCount];
            //Element (id, viscosity, density)
           
            //vapor
            elements[0] = new Element(0, -1, 0.49f);
            elements[0].setFreezeTransition(50f,2);
            //oil
            elements[1] = new Element(1, 2, 1f);
            //water
            elements[2] = new Element(2, 0, 2f);
            elements[2].setEvaporateTransition(100f, 0);
        }
    }
    
}