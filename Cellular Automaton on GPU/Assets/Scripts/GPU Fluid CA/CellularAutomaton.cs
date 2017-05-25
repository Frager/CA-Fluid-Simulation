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
        public ComputeShader cs;

        public ComputeShader rbInteraction;
        public Material rbMaterial;
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
            for (int i = 0; i < 8; ++i)
            {
                KernelOrder[i] = cs.FindKernel(FunctionOrder[i]);
            }

            threadGroups = new int[][] { new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 }, new int[] { dimensions.x, dimensions.y, dimensions.z * 2 } };

            buffer = new ComputeBuffer[] { new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, (elementCount + 2) * sizeof(float), ComputeBufferType.GPUMemory), new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, (elementCount + 2) * sizeof(float), ComputeBufferType.GPUMemory) };

            InitializeComputeShader();

            visualization.Initialize(dimensions);

            rbInteraction.SetInts("size", new int[] { dimensions.x * 16, dimensions.y * 16, dimensions.z * 16 });

            rigidBodies = new ComputeBuffer(1, sizeof(float) * 6);
            rigidBodies.SetData(new float[] { 1, 1, 1, 15, 15, 12 });

            queryResult = new ComputeBuffer(1, sizeof(float));
            queryResult.SetData(new float[] { 0f });
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

        public void RigidBodyInteraction()
        {
            int kernelHandle = rbInteraction.FindKernel("CSMain2");

            rbInteraction.SetBuffer(kernelHandle, "newPositions2", rigidBodies);
            rbInteraction.SetBuffer(kernelHandle, "currentGeneration", buffer[updateCycle % 2]);

            rbInteraction.Dispatch(kernelHandle, 1, 1, 1);

            //float[] data = new float[3];
            //position.GetData(data);

            rbMaterial.SetBuffer("newPositions2", rigidBodies);
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
                visualization.Render(buffer[updateCycle % 2]);

            updateCycle = (updateCycle + 1) % 8;
        }

        //Floatables
        public float getFluidHeightAtCoordinate(int[] coordinate, float density)
        {
            //ask compute shader
            if (coordinate.Length != 3)
            {
                print("Coordinate length != 3");
                return float.MinValue;
            }
            int kernelHandle = cs.FindKernel("GetHeight");
            cs.SetBuffer(kernelHandle, "queryResult", queryResult);
            cs.SetInts("queryCellCoord", coordinate);
            cs.SetFloat("queryDensity", density);
            cs.Dispatch(kernelHandle, 1, 1, 1);
            float[] result = new float[1];
            queryResult.GetData(result);
            return result[0];
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
            queryResult.Release();
            rigidBodies.Release();
        }
    }
}
