#define TRIANGLES

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

        #if CUBES
        private ComputeBuffer quads;
        #else
        private ComputeBuffer triangles;
        #endif

        private ComputeBuffer args;
        private int[] data;

        private int updateCycle = 0;

        private string[] FunctionOrder = { "UpdateX", "UpdateY", "UpdateZ", "UpdateY", "UpdateX", "UpdateY", "UpdateZ", "UpdateY" };
        private int[] KernelOrder = new int[8];
        private int[][] offset = { new int[]{ 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 } };
        private int[][] threadGroups;

        void Start()
        {
            texture3D = new RenderTexture(gridSize, gridSize, 1);
            texture3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture3D.volumeDepth = gridSize;
            texture3D.enableRandomWrite = true;
            texture3D.Create();
            testMaterial.SetTexture("_MainTex", texture3D);

            for (int i = 0; i < 8; ++i)
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
#if CUBES
            quads.Release();
#else
            triangles.Release();
#endif
            args.Release();
        }

        void InitializeBuffers()
        {
            threadGroups = new int[][]{ new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 } };

            buffer = new ComputeBuffer[] { new ComputeBuffer(gridSize * gridSize * gridSize, (elementCount + 1) * sizeof(int) + sizeof(float), ComputeBufferType.GPUMemory), new ComputeBuffer(gridSize * gridSize * gridSize, (elementCount + 1) * sizeof(int) + sizeof(float), ComputeBufferType.GPUMemory) };

            computeShader.SetInt("size", gridSize);
            computeShader.SetInt("maxVolume", maxVolume);

            int kernelHandle = computeShader.FindKernel("Initialize");

            computeShader.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            computeShader.Dispatch(kernelHandle, gridSize / 16, gridSize / 8, gridSize / 8);

            computeShader.SetBuffer(kernelHandle, "newGeneration", buffer[1]);
            computeShader.Dispatch(kernelHandle, gridSize / 16, gridSize / 8, gridSize / 8);

            args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            data = new int[4] { 0, 1, 0, 0 };
            args.SetData(data);

            marchingCubesCS.SetFloat("scale", scale);
            marchingCubesCS.SetInt("size", gridSize);
            marchingCubesCS.SetInt("maxVolume", maxVolume);

#if CUBES
            quads = new ComputeBuffer(gridSize * gridSize * gridSize, 4 * 3 * sizeof(float), ComputeBufferType.Append);
            ComputeBuffer.CopyCount(quads, args, 0);
#else
            triangles = new ComputeBuffer(gridSize * gridSize * gridSize, 3 * 3 * sizeof(float), ComputeBufferType.Append);
            ComputeBuffer.CopyCount(triangles, args, 0);
#endif

            args.GetData(data);

            CA2Texture3D.SetInt("size", gridSize);
            CA2Texture3D.SetInt("maxVolume", maxVolume);

            testMaterial.SetFloat("scale", scale);
            testMaterial.SetFloat("size", gridSize);
        }

        private float timer = 0;
        private float timeframe = 0.01f;

        private int timeCounter = 0;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                if(timeCounter == 100)
                {
                    Application.Quit();
                }

                NextGeneration();

                Render();

                timer -= timeframe;
                updateCycle = (updateCycle + 1) % 8;

                ++timeCounter;
            }
        }

        void NextGeneration()
        {
            computeShader.SetBuffer(KernelOrder[updateCycle], "newGeneration", buffer[updateCycle % 2]);
            computeShader.SetBuffer(KernelOrder[updateCycle], "currentGeneration", buffer[(updateCycle + 1) % 2]);
            computeShader.SetInts("fill", new int[] { x, y, z, element });
            computeShader.SetInts("offset", offset[updateCycle]);
            computeShader.Dispatch(KernelOrder[updateCycle], threadGroups[updateCycle][0], threadGroups[updateCycle][1], threadGroups[updateCycle][2]);
        }

        void Render()
        {
            int kernelHandle = CA2Texture3D.FindKernel("CSMain");

            CA2Texture3D.SetBuffer(kernelHandle, "currentGeneration", buffer[updateCycle % 2]);
            CA2Texture3D.SetTexture(kernelHandle, "Result", texture3D);
            CA2Texture3D.Dispatch(kernelHandle, gridSize / 16, gridSize / 8, gridSize / 8);

            marchingCubesCS.SetInt("size", gridSize);
            marchingCubesCS.SetInt("maxVolume", maxVolume);
#if CUBES
            quads.SetCounterValue(0);
            marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "cubes", quads);
#else
            triangles.SetCounterValue(0);
            marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "triangles", triangles);
#endif
            marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "currentGeneration", buffer[updateCycle % 2]);
            marchingCubesCS.Dispatch(marchingCubesCS.FindKernel("CSMain"), gridSize / 16, gridSize / 8, gridSize / 8);


            Vector4 waveSpeed = testMaterial.GetVector("WaveSpeed");
            float waveScale = testMaterial.GetFloat("_WaveScale");
            float t = Time.time / 20.0f;

            Vector4 offset4 = waveSpeed * (t * waveScale);
            Vector4 offsetClamped = new Vector4(Mathf.Repeat(offset4.x, 1.0f), Mathf.Repeat(offset4.y, 1.0f),
            Mathf.Repeat(offset4.z, 1.0f), Mathf.Repeat(offset4.w, 1.0f));
            testMaterial.SetVector("_WaveOffset", offsetClamped);
        }

        private void OnPostRender()
        {
            testMaterial.SetPass(0);
#if CUBES
            ComputeBuffer.CopyCount(quads, args, 0);
            testMaterial.SetBuffer("quads", quads);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, args);
#else
            ComputeBuffer.CopyCount(triangles, args, 0);
            testMaterial.SetBuffer("triangles", triangles);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, args);
#endif
        }
    }

    public class CellularAutomaton
    {
        private ComputeShader cs;

        private int gridSize;
        private int maxVolume;
        private int elementCount;

        private ComputeBuffer[] buffer;
        private int updateCycle = 0;

        private string[] FunctionOrder = { "UpdateX", "UpdateY", "UpdateZ", "UpdateY", "UpdateX", "UpdateY", "UpdateZ", "UpdateY" };
        private int[] KernelOrder = new int[8];
        private int[][] offset = { new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 1, 0 }, new int[] { 1, 0, 0 }, new int[] { 0, 0, 0 }, new int[] { 0, 0, 1 }, new int[] { 0, 1, 0 } };
        private int[][] threadGroups;

        public CellularAutomaton(ComputeShader cs, int gridSize, Cell cell)
        {
            this.cs = cs;
            this.gridSize = gridSize;
            maxVolume = Cell.maxVolume;
            elementCount = cell.content.Length;

            InitializeComputeBuffer();
            InitializeComputeShader();
        }

        private void InitializeComputeBuffer()
        {
            for (int i = 0; i < 8; ++i)
            {
                KernelOrder[i] = cs.FindKernel(FunctionOrder[i]);
            }

            threadGroups = new int[][] { new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 }, new int[] { gridSize / 16, gridSize / 16, gridSize / 8 } };

            buffer = new ComputeBuffer[] { new ComputeBuffer(gridSize * gridSize * gridSize, (elementCount + 1) * sizeof(int) + sizeof(float), ComputeBufferType.GPUMemory), new ComputeBuffer(gridSize * gridSize * gridSize, (elementCount + 1) * sizeof(int) + sizeof(float), ComputeBufferType.GPUMemory) };
        }

        private void InitializeComputeShader()
        {
            cs.SetInt("size", gridSize);
            cs.SetInt("maxVolume", maxVolume);

            int kernelHandle = cs.FindKernel("Initialize");

            cs.SetBuffer(kernelHandle, "newGeneration", buffer[0]);
            cs.Dispatch(kernelHandle, gridSize / 16, gridSize / 8, gridSize / 8);

            cs.SetBuffer(kernelHandle, "newGeneration", buffer[1]);
            cs.Dispatch(kernelHandle, gridSize / 16, gridSize / 8, gridSize / 8);
        }

        public void Fill(int[] fill)
        {
            cs.SetInts("fill", fill);
        }

        public void NextGeneration()
        {
            cs.SetBuffer(KernelOrder[updateCycle], "newGeneration", buffer[updateCycle % 2]);
            cs.SetBuffer(KernelOrder[updateCycle], "currentGeneration", buffer[(updateCycle + 1) % 2]);
            cs.SetInts("offset", offset[updateCycle]);
            cs.Dispatch(KernelOrder[updateCycle], threadGroups[updateCycle][0], threadGroups[updateCycle][1], threadGroups[updateCycle][2]);
            updateCycle = (updateCycle + 1) % 8;
        }

        ~CellularAutomaton()
        {
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i].Release();
            }
        }
    }

    public class Cell
    {
        public int[] content;
        public int volume;
        public static int maxVolume;
    }

    public class Visualization
    {
        private float scale;
        private int gridSize;
        private int maxVolume;

        private ComputeShader texture3DCS;
        private int texture3DCSKernel;
        private ComputeShader marchingCubesCS;
        private int marchingCubesCSKernel;

#if CUBES
        private ComputeBuffer quads;
#else
        private ComputeBuffer triangles;
#endif

        private ComputeBuffer args;
        private int[] data;

        private Material material;

        private RenderTexture texture3D;

        public Visualization(int gridSize, float scale)
        {
            this.scale = scale;
            this.gridSize = gridSize;
            maxVolume = Cell.maxVolume;
        }

        private void InitializeComputeBuffer()
        {
            args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            data = new int[4] { 0, 1, 0, 0 };
            args.SetData(data);

#if CUBES
            quads = new ComputeBuffer(gridSize * gridSize * gridSize, 4 * 3 * sizeof(float), ComputeBufferType.Append);
            ComputeBuffer.CopyCount(quads, args, 0);
#else
            triangles = new ComputeBuffer(gridSize * gridSize * gridSize, 3 * 3 * sizeof(float), ComputeBufferType.Append);
            ComputeBuffer.CopyCount(triangles, args, 0);
#endif
        }

        private void InitializeShader()
        {
            marchingCubesCS.SetFloat("scale", scale);
            marchingCubesCS.SetInt("size", gridSize);
            marchingCubesCS.SetInt("maxVolume", maxVolume);

            texture3DCS.SetInt("size", gridSize);
            texture3DCS.SetInt("maxVolume", maxVolume);

            material.SetFloat("scale", scale);
            material.SetFloat("size", gridSize);
        }

        private void RenderRealisticWater()
        {
            Vector4 waveSpeed = material.GetVector("WaveSpeed");
            float waveScale = material.GetFloat("_WaveScale");
            float t = Time.time / 20.0f;

            Vector4 offset4 = waveSpeed * (t * waveScale);
            Vector4 offsetClamped = new Vector4(Mathf.Repeat(offset4.x, 1.0f), Mathf.Repeat(offset4.y, 1.0f),
            Mathf.Repeat(offset4.z, 1.0f), Mathf.Repeat(offset4.w, 1.0f));
            material.SetVector("_WaveOffset", offsetClamped);
        }

        private void RenderTexture3D(ComputeBuffer cells)
        {
            texture3DCS.SetBuffer(texture3DCSKernel, "currentGeneration", cells);
            texture3DCS.SetTexture(texture3DCSKernel, "Result", texture3D);
            texture3DCS.Dispatch(texture3DCSKernel, gridSize / 16, gridSize / 8, gridSize / 8);
        }

        public void Render(ComputeBuffer cells)
        {
#if CUBES
            quads.SetCounterValue(0);
            marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "cubes", quads);
#else
            triangles.SetCounterValue(0);
            marchingCubesCS.SetBuffer(marchingCubesCSKernel, "triangles", triangles);
#endif
            marchingCubesCS.SetBuffer(marchingCubesCSKernel, "currentGeneration", cells);
            marchingCubesCS.Dispatch(marchingCubesCSKernel, gridSize / 16, gridSize / 8, gridSize / 8);

            RenderTexture3D(cells);
            RenderRealisticWater();
        }

        public void OnRender()
        {
            material.SetPass(0);
#if CUBES
            ComputeBuffer.CopyCount(quads, args, 0);
            testMaterial.SetBuffer("quads", quads);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, args);
#else
            ComputeBuffer.CopyCount(triangles, args, 0);
            material.SetBuffer("triangles", triangles);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, args);
#endif
        }

        ~Visualization()
        {
#if CUBES
            quads.Release();
#else
            triangles.Release();
#endif
            args.Release();
    }
}
}
