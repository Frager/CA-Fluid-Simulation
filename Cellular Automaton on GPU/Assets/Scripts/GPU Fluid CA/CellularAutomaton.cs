using UnityEngine;

namespace GPUFluid
{
    //The size of the cellular automaton
    [System.Serializable]
    public struct GridDimensions
    {
        [Dimension()]
        public int x, y, z;
    }

    public class CellularAutomaton : MonoBehaviour
    {
        //Compute Shader with update rules
        public ComputeShader cellularAutomaton;

        private ComputeBuffer rigidBodies;
        private ComputeBuffer queryResult;

        public GridDimensions dimensions;

        //The number of different elements in the simulation
        //Warning: This value must be identical to that in the shaders
        private int elementCount = 3;

        //The visualisation for this cellular automaton
        public GPUVisualisation visualization;

        //This cellular automaton consists of two buffers: One for the current state (read) and one for the next generation (write).
        //After an update the buffers are swapped.
        private ComputeBuffer[] buffer;
        private int updateCycle = 0;

        //The order of the update rules
        private string[] FunctionOrder = { "UpdateX", "UpdateY", "UpdateZ", "UpdateY", "UpdateX", "UpdateY", "UpdateZ", "UpdateY" };

        private int[] KernelOrder = new int[8];
        private int[][] offset = { new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 } };
        private int[][] threadGroups;


        void Awake()
        {
            for (int i = 0; i < 8; ++i)
            {
                KernelOrder[i] = cellularAutomaton.FindKernel(FunctionOrder[i]);
            }

            threadGroups = new int[][] { new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 } };

            buffer = new ComputeBuffer[] { new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, (elementCount + 2) * sizeof(float), ComputeBufferType.GPUMemory), new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, (elementCount + 2) * sizeof(float), ComputeBufferType.GPUMemory) };

            InitializeComputeBuffer();

            visualization.Initialize(dimensions);

            rigidBodies = new ComputeBuffer(1, sizeof(float) * 6);
            rigidBodies.SetData(new float[] { 1, 1, 1, 15, 15, 12 });

        }


        private void InitializeComputeBuffer()
        {
            int kernelHandle = cellularAutomaton.FindKernel("Initialize");

            cellularAutomaton.SetInts("size", new int[] { dimensions.x * 16, dimensions.y * 16, dimensions.z * 16 });

            cellularAutomaton.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            cellularAutomaton.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);

            cellularAutomaton.SetBuffer(kernelHandle, "newGeneration", buffer[1]);
            cellularAutomaton.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);
        }

        public void SetObstacle(int[] obstacleStart, int[] obstacleEnd)
        {
            int kernelHandle = cellularAutomaton.FindKernel("SetObstacle");

            cellularAutomaton.SetInts("obstacleStart", obstacleStart);
            cellularAutomaton.SetInts("obstacleEnd", obstacleEnd);

            cellularAutomaton.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            cellularAutomaton.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);

            cellularAutomaton.SetBuffer(kernelHandle, "newGeneration", buffer[1]);
            cellularAutomaton.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);
        }

        public void RemoveObstacle(int[] obstacleStart, int[] obstacleEnd)
        {
            int kernelHandle = cellularAutomaton.FindKernel("RemoveObstacle");

            cellularAutomaton.SetInts("obstacleStart", obstacleStart);
            cellularAutomaton.SetInts("obstacleEnd", obstacleEnd);

            cellularAutomaton.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            cellularAutomaton.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);

            cellularAutomaton.SetBuffer(kernelHandle, "newGeneration", buffer[1]);
            cellularAutomaton.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);
        }

        /// <summary>
        /// This function determines where and whether fluid will be filled in the cellular automaton.
        /// </summary>
        /// <param name="fill">An array of size 4. The first three values determine the position, where fluid will be filled in (a coordinate outside the borders will stop filling). The last value determines the element-type.</param>
        public void Fill(float[] fill, int element)
        {
            cellularAutomaton.SetInts("fill", new int[] { (int)(fill[0] * dimensions.x * 16.0), (int)(fill[1] * dimensions.y * 16.0), (int)(fill[2] * dimensions.z * 16.0), element });
        }

        /// <summary>
        /// This function determines where and how much a cell is heaten.
        /// </summary>
        /// <param name="heat">An array of size 4. The first three values determine the position and the last value determines the temperature in degree Celsius.</param>
        public void Heat(int[] heat)
        {
            cellularAutomaton.SetInts("heat", heat);
        }

        /// <summary>
        /// Computes the next generation.
        /// </summary>
        public void NextGeneration()
        {
            cellularAutomaton.SetBuffer(KernelOrder[updateCycle], "newGeneration", buffer[updateCycle % 2]);
            cellularAutomaton.SetBuffer(KernelOrder[updateCycle], "currentGeneration", buffer[(updateCycle + 1) % 2]);
            cellularAutomaton.SetInts("offset", offset[updateCycle]);
            cellularAutomaton.Dispatch(KernelOrder[updateCycle], threadGroups[updateCycle][0], threadGroups[updateCycle][1], threadGroups[updateCycle][2]);

            if (updateCycle % 2 == 0)
            {
                visualization.Render(buffer[updateCycle % 2]);
            }

            updateCycle = (updateCycle + 1) % 8;
        }

        /// <summary>
        /// Initializes ComputeBuffer for floatable calculation
        /// </summary>
        /// <param name="numFloatables">Number of total Floatables in szene</param>
        public void initializeFloatableBuffer(int numFloatables)
        {
            queryResult = new ComputeBuffer(numFloatables * 4, sizeof(float));
        }

        /// <summary>
        /// returns the liquid height for each grid coordinate in coordinates list.
        /// </summary>
        /// <param name="coordinates">List of coordinates. one Coordinate consists of three subsequent int values</param>
        /// <param name="density"> maximum densities of fluids. Fluids with lower densities will be ignored for height calculation</param>
        /// <returns></returns>
        public float[] getFluidHeightsAtCoordinates(int[] coordinates, float [] densities)
        {
            if (coordinates.Length % 3 != 0)
            {
                if(coordinates.Length / 3 != densities.Length)
                {
                    print("Coordinates / 3 != densities");
                    return null;
                }
                print("Coordinates % 3 != 0");
                return null;
            }

            //querry format: [grid x, grid y, grid z, density]*
            float[] querry = new float[coordinates.Length + densities.Length];

            for(int i = 0; i < coordinates.Length / 3; i++)
            {
                querry[i * 4] = coordinates[i * 3];
                querry[i * 4 +1] = coordinates[i * 3 +1];
                querry[i * 4 +2] = coordinates[i * 3 +2];
                querry[i * 4 +3] = densities[i];
            }
            
            queryResult.SetData(querry);
            int kernelHandle = cellularAutomaton.FindKernel("GetHeight");
            cellularAutomaton.SetBuffer(kernelHandle, "queryResult", queryResult);
            cellularAutomaton.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            //cs.SetInts("queryCellCoord", coordinates);
            //cs.SetFloat("queryDensity", density);
            cellularAutomaton.Dispatch(kernelHandle, coordinates.Length / 3, 1, 1);
            //float[] result = new float[1];
            queryResult.GetData(querry);
            return querry;
        }

        /// <summary>
        /// Don't forget releasing the buffers.
        /// </summary>
        void OnDisable()
        {
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i].Release();
            }
            if(queryResult != null)
                queryResult.Release();
            rigidBodies.Release();
        }
    }
}
