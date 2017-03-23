using UnityEngine;

namespace GPUFluid
{
    public class CellularAutomaton : MonoBehaviour
    {
        public ComputeShader computeShader;

        public ComputeShader CA2Texture3D;

        public Material testMaterial;

        private RenderTexture cellBuffer1;
        private RenderTexture cellBuffer2;
        private int buffer = 1;

        public static int size = 8;

        private RenderTexture texture3D;

        public SimpleVisuals visuals;

        void Start()
        {
            cellBuffer1 = new RenderTexture(size, size, 1, RenderTextureFormat.RGInt);
            cellBuffer1.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            cellBuffer1.volumeDepth = size;
            cellBuffer1.enableRandomWrite = true;
            cellBuffer1.Create();

            cellBuffer2 = new RenderTexture(size, size, 1, RenderTextureFormat.RGInt);
            cellBuffer2.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            cellBuffer2.volumeDepth = size;
            cellBuffer2.enableRandomWrite = true;
            cellBuffer2.Create();

            texture3D = new RenderTexture(size, size, 1);
            texture3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture3D.volumeDepth = size;
            texture3D.enableRandomWrite = true;
            texture3D.Create();
            testMaterial.SetTexture("_MainTex", texture3D);

            StartComputeShader();
            visuals.GenerateVisuals(transform.position, size, size, size);
            InvokeRepeating("NextGeneration", 0, 0.5f);
        }

        public void NextGeneration()
        {
            FillComputeShader(new Vector4(1, size - 1, 1, 1));
            UpdateMethod();
            Render();
            buffer = (buffer == 1) ? 2 : 1;
        }

        void StartComputeShader()
        {
            int kernelHandle = computeShader.FindKernel("Initialize");

            computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer2);

            computeShader.Dispatch(kernelHandle, size / 4, size / 4, size / 4);

            computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer1);

            computeShader.Dispatch(kernelHandle, size / 4, size / 4, size / 4);
        }

        public void FillComputeShader(Vector4 vector)
        {
            int kernelHandle = computeShader.FindKernel("Fill");

            if (buffer == 1)
                computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer2);
            else
                computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer1);

            computeShader.SetVector("fill", vector);

            computeShader.Dispatch(kernelHandle, 1, 1, 1);
        }

        void Render()
        {
            int kernelHandle = CA2Texture3D.FindKernel("CSMain");

            if (buffer == 1)
                CA2Texture3D.SetTexture(kernelHandle, "Cells", cellBuffer1);
            else
                CA2Texture3D.SetTexture(kernelHandle, "Cells", cellBuffer2);

            CA2Texture3D.SetTexture(kernelHandle, "Result", texture3D);

            CA2Texture3D.Dispatch(kernelHandle, size / 4, size / 4, size / 4);
        }

        void UpdateMethod()
        {
            int kernelHandle = computeShader.FindKernel("UpdateGive");

            if (buffer == 1)
            {
                computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer1);
                computeShader.SetTexture(kernelHandle, "OldCells", cellBuffer2);
            }
            else
            {
                computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer2);
                computeShader.SetTexture(kernelHandle, "OldCells", cellBuffer1);
            }

            computeShader.Dispatch(kernelHandle, size / 4, size / 4, size / 4);

            buffer = (buffer == 1) ? 2 : 1;

            kernelHandle = computeShader.FindKernel("UpdateGet");

            if (buffer == 1)
            {
                computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer1);
                computeShader.SetTexture(kernelHandle, "OldCells", cellBuffer2);
            }
            else
            {
                computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer2);
                computeShader.SetTexture(kernelHandle, "OldCells", cellBuffer1);
            }

            computeShader.Dispatch(kernelHandle, size / 4, size / 4, size / 4);
        }
    }
}
