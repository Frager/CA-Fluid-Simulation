using UnityEngine;

namespace GPUFluid
{
    public class ScreenSpaceFluidRendering : GPUVisualisation
    {
        public Material blend;

        private RenderTexture depthTex;
        private RenderTexture blurTex;

        private ComputeShader cs;
        private int cs2Mesh;
        private int blur;

        private Camera realCam;

        private int[] renderTextureSize = new int[] { 512, 512 };

        override protected void InitializeComputeBuffer()
        {
            args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            data = new int[4] { 0, 1, 0, 0 };
            args.SetData(data);

            mesh = new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, 4 * sizeof(float), ComputeBufferType.Append);

            ComputeBuffer.CopyCount(mesh, args, 0);
        }

        override protected void InitializeComputeShader()
        {
            cs = Resources.Load<ComputeShader>("ComputeShader/ScreenSpaceFluidRendering/ScreenSpaceFluidRendering");
            cs.SetInts("size", new int[] { dimensions.x * 16, dimensions.y * 16, dimensions.z * 16 });

            cs2Mesh = cs.FindKernel("CA2Mesh");
            blur = cs.FindKernel("HorizontalBlur");

            cs.SetInts("blurDir", new int[] { 1, 0 });

            cs.SetBuffer(cs2Mesh, "mesh", mesh);
        }

        override protected void InitializeMaterial()
        {
            depthTex = new RenderTexture(renderTextureSize[0], renderTextureSize[1], 24, RenderTextureFormat.RFloat);
            depthTex.enableRandomWrite = true;
            depthTex.Create();
            GetComponent<Camera>().targetTexture = depthTex;
            GetComponent<Camera>().cullingMask = 0;

            blurTex = new RenderTexture(renderTextureSize[0], renderTextureSize[1], 0, RenderTextureFormat.RFloat);
            blurTex.enableRandomWrite = true;
            blurTex.Create();

            cs.SetTexture(blur, "DepthTex", depthTex);
            cs.SetTexture(blur, "BlurTex", blurTex);

            blend.SetTexture("_Overlay", depthTex);

            material = new Material(Resources.Load<Shader>("Shader/ScreenSpaceFluidRendering/ScreenSpaceFluidRendering"));
        }


        override public void Render(ComputeBuffer cells)
        {
            mesh.SetCounterValue(0);
            cs.SetBuffer(cs2Mesh, "currentGeneration", cells);
            cs.Dispatch(cs2Mesh, dimensions.x, dimensions.y * 2, dimensions.z * 2);

            //cs.Dispatch(blur, renderTextureSize[0] / 4, renderTextureSize[1] / 256, 1);
            //cs.Dispatch(blur, renderTextureSize[0] / 32, renderTextureSize[1] / 32, 1);

            ComputeBuffer.CopyCount(mesh, args, 0);
        }

        void OnDisable()
        {
            mesh.Release();
            args.Release();
        }
    }
}