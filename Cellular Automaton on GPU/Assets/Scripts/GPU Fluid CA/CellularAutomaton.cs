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
        public ScreenSpaceFluidVisualisation ssfv;

        //Compute Shader with update rules
        public ComputeShader cs;

        private ComputeBuffer rigidBodies;
        private ComputeBuffer queryResult;

        public GridDimensions dimensions;

        //The number of different elements in the simulation
        //Warning: This value must be identical to that in the shaders
        private int elementCount = 3;

        //The visualisation for this cellular automaton
        public MarchingCubesVisualisation visualization;

        //This cellular automaton consists of two buffers: One for the current state (read) and one for the next generation (write).
        //After an update the buffers are swapped.
        private ComputeBuffer[] buffer;
        private int updateCycle = 0;

        //The order of the update rules
        private string[] FunctionOrder = { "UpdateX", "UpdateY", "UpdateZ", "UpdateY", "UpdateX", "UpdateY", "UpdateZ", "UpdateY" };

        private int[] KernelOrder = new int[8];
        private int[][] offset = { new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 } };
        private int[][] threadGroups;


        void Start()
        {
            if(ssfv != null)
                ssfv.Initialize(dimensions);

            for (int i = 0; i < 8; ++i)
            {
                KernelOrder[i] = cs.FindKernel(FunctionOrder[i]);
            }

            threadGroups = new int[][] { new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 } };

            buffer = new ComputeBuffer[] { new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, (elementCount + 2) * sizeof(float), ComputeBufferType.GPUMemory), new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, (elementCount + 2) * sizeof(float), ComputeBufferType.GPUMemory) };

            InitializeComputeShader();

            visualization.Initialize(dimensions);

            rigidBodies = new ComputeBuffer(1, sizeof(float) * 6);
            rigidBodies.SetData(new float[] { 1, 1, 1, 15, 15, 12 });

        }


        private void InitializeComputeShader()
        {
            int kernelHandle = cs.FindKernel("Initialize");

            cs.SetInts("size", new int[] { dimensions.x * 16, dimensions.y * 16, dimensions.z * 16 });

            cs.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            cs.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);

            cs.SetBuffer(kernelHandle, "newGeneration", buffer[1]);
            cs.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);
        }

        public void SetObstacle(int[] obstacleStart, int[] obstacleEnd)
        {
            int kernelHandle = cs.FindKernel("SetObstacle");

            cs.SetInts("obstacleStart", obstacleStart);
            cs.SetInts("obstacleEnd", obstacleEnd);

            cs.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            cs.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);

            cs.SetBuffer(kernelHandle, "newGeneration", buffer[1]);
            cs.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);
        }

        public void RemoveObstacle(int[] obstacleStart, int[] obstacleEnd)
        {
            int kernelHandle = cs.FindKernel("RemoveObstacle");

            cs.SetInts("obstacleStart", obstacleStart);
            cs.SetInts("obstacleEnd", obstacleEnd);

            cs.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            cs.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);

            cs.SetBuffer(kernelHandle, "newGeneration", buffer[1]);
            cs.Dispatch(kernelHandle, dimensions.x, dimensions.y * 2, dimensions.z * 2);
        }

        /// <summary>
        /// This function determines where and whether fluid will be filled in the cellular automaton.
        /// </summary>
        /// <param name="fill">An array of size 4. The first three values determine the position, where fluid will be filled in (a coordinate outside the borders will stop filling). The last value determines the element-type.</param>
        public void Fill(float[] fill, int element)
        {
            cs.SetInts("fill", new int[] { (int)(fill[0] * dimensions.x * 16.0), (int)(fill[1] * dimensions.y * 16.0), (int)(fill[2] * dimensions.z * 16.0), element });
        }

        /// <summary>
        /// This function determines where and how much a cell is heaten.
        /// </summary>
        /// <param name="heat">An array of size 4. The first three values determine the position and the last value determines the temperature in degree Celsius.</param>
        public void Heat(int[] heat)
        {
            cs.SetInts("heat", heat);
        }

        /// <summary>
        /// Computes the next generation.
        /// </summary>
        public void NextGeneration()
        {
            cs.SetBuffer(KernelOrder[updateCycle], "newGeneration", buffer[updateCycle % 2]);
            cs.SetBuffer(KernelOrder[updateCycle], "currentGeneration", buffer[(updateCycle + 1) % 2]);
            cs.SetInts("offset", offset[updateCycle]);
            cs.Dispatch(KernelOrder[updateCycle], threadGroups[updateCycle][0], threadGroups[updateCycle][1], threadGroups[updateCycle][2]);

            if (updateCycle % 2 == 0)
            {
                visualization.Render(buffer[updateCycle % 2]);
                if (ssfv != null)
                    ssfv.Render(buffer[updateCycle % 2]);
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
            int kernelHandle = cs.FindKernel("GetHeight");
            cs.SetBuffer(kernelHandle, "queryResult", queryResult);
            cs.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            //cs.SetInts("queryCellCoord", coordinates);
            //cs.SetFloat("queryDensity", density);
            cs.Dispatch(kernelHandle, coordinates.Length / 3, 1, 1);
            //float[] result = new float[1];
            queryResult.GetData(querry);
            return querry;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (ssfv != null)
            {
                var p = GL.GetGPUProjectionMatrix(GetComponent<Camera>().projectionMatrix, false);
                p[2, 3] = p[3, 2] = 0.0f;
                p[3, 3] = 1.0f;
                var clipToWorld = Matrix4x4.Inverse(p * GetComponent<Camera>().worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
                ssfv.blend.SetMatrix("clipToWorld", clipToWorld);
                ssfv.blend.SetMatrix("viewProj", GetComponent<Camera>().worldToCameraMatrix * GetComponent<Camera>().projectionMatrix);
                Graphics.Blit(source, destination, ssfv.blend);
            }
            else
                Graphics.Blit(source, destination);
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
