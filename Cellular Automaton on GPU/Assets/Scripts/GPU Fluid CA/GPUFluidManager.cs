using UnityEngine;

namespace GPUFLuid
{
    public class GPUFluidManager : MonoBehaviour
    {
        public int gridSize = 16;
        public int maxVolume;
        public int elementCount;

        public float scale = 1;

        public ComputeShader computeShader;
        public ComputeShader CA2Texture3D;
        public ComputeShader marchingCubesCS;

        public Material testMaterial;

        [Range(0, 14)]
        public int x, y, z;

        [Range(0, 2)]
        public int element;

        private RenderTexture texture3D;

        private ComputeBuffer[] buffer;

        private ComputeBuffer quads;
        private ComputeBuffer args;
        private int[] data;

        private int updateCycle = 0;

        private string[] FunctionOrder = { "UpdateX", "UpdateY", "UpdateZ", "UpdateY", "UpdateX", "UpdateY", "UpdateZ", "UpdateY" };
        private int[] KernelOrder = new int[8];
        private int[][] offset = { new int[]{ 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 } };

        void Start()
        {
            texture3D = new RenderTexture(gridSize, gridSize, 1);
            texture3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture3D.volumeDepth = gridSize;
            texture3D.enableRandomWrite = true;

            texture3D.Create();
            testMaterial.SetTexture("_MainTex", texture3D);

            for(int i = 0; i < 8; ++i)
            {
                KernelOrder[i] = computeShader.FindKernel(FunctionOrder[i]);
            }

            InitializeBuffers();
            Render();
        }

        private void OnDisable()
        {
            for(int i = 0; i < buffer.Length; ++i)
            {
                buffer[i].Release();
            }
            quads.Release();
            args.Release();
        }

        void InitializeBuffers()
        {
            buffer = new ComputeBuffer[] { new ComputeBuffer(gridSize * gridSize * gridSize, (elementCount + 1) * sizeof(int) + sizeof(float), ComputeBufferType.GPUMemory), new ComputeBuffer(gridSize * gridSize * gridSize, (elementCount + 1) * sizeof(int) + sizeof(float), ComputeBufferType.GPUMemory) };

            computeShader.SetInt("size", gridSize);
            computeShader.SetInt("maxVolume", maxVolume);

            int kernelHandle = computeShader.FindKernel("Initialize");

            computeShader.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            computeShader.Dispatch(kernelHandle, gridSize / 8, gridSize / 8, gridSize / 8);

            computeShader.SetBuffer(kernelHandle, "newGeneration", buffer[1]);
            computeShader.Dispatch(kernelHandle, gridSize / 8, gridSize / 8, gridSize / 8);

            args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            data = new int[4] { 0, 1, 0, 0 };
            args.SetData(data);

            marchingCubesCS.SetFloat("scale", scale);
            marchingCubesCS.SetInt("size", gridSize);
            marchingCubesCS.SetInt("maxVolume", maxVolume);

            quads = new ComputeBuffer(gridSize * gridSize * gridSize, 4 * 3 * sizeof(float), ComputeBufferType.Append);

            ComputeBuffer.CopyCount(quads, args, 0);
            args.GetData(data);

            CA2Texture3D.SetInt("size", gridSize);
            CA2Texture3D.SetInt("maxVolume", maxVolume);

            testMaterial.SetFloat("scale", scale);
            testMaterial.SetFloat("size", gridSize);
        }

        private float timer = 0;
        private float timeframe = 0.01f;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                computeShader.SetBuffer(KernelOrder[updateCycle], "newGeneration", buffer[updateCycle % 2]);
                computeShader.SetBuffer(KernelOrder[updateCycle], "currentGeneration", buffer[(updateCycle + 1) % 2]);
                computeShader.SetInts("fill", new int[] { x, y, z, element });
                computeShader.SetInts("offset", offset[updateCycle]);
                computeShader.Dispatch(KernelOrder[updateCycle], gridSize / 8, gridSize / 8, gridSize / 8);
                Render();

                timer -= timeframe;
                updateCycle = (updateCycle + 1) % 8;
            }
        }

        void Render()
        {
            int kernelHandle = CA2Texture3D.FindKernel("CSMain");

            CA2Texture3D.SetBuffer(kernelHandle, "currentGeneration", buffer[updateCycle % 2]);
            CA2Texture3D.SetTexture(kernelHandle, "Result", texture3D);
            CA2Texture3D.Dispatch(kernelHandle, gridSize / 8, gridSize / 8, gridSize / 8);

            quads.SetCounterValue(0);
            marchingCubesCS.SetInt("size", gridSize);
            marchingCubesCS.SetInt("maxVolume", maxVolume);
            marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "quads", quads);
            marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "currentGeneration", buffer[updateCycle % 2]);
            marchingCubesCS.Dispatch(marchingCubesCS.FindKernel("CSMain"), gridSize / 8, gridSize / 8, gridSize / 8);
        }

        private void OnPostRender()
        {
            ComputeBuffer.CopyCount(quads, args, 0);
            testMaterial.SetPass(0);
            testMaterial.SetBuffer("quads", quads);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, args);
        }
    }
}
