//#define REALISTIC

using UnityEngine;

namespace GPUFluid
{
    public enum Type
    {
        CUBES, SIMPLE, TESSELATION, MULTIPLE_FLUIDS
    }

    public enum Shading
    {
        FLAT, GOURAUD, PHONG
    }

    /// <summary>
    /// This class executes a Marching Cubes algorithm on the data of a CellularAutomaton.
    /// At the moment there are two possible visualisations:
    /// The CUBES visualisation creates a voxelised mesh.
    /// The other types create a more complex mesh.
    /// </summary>
    public class MarchingCubesVisualisation : GPUVisualisation
    {
        //The basic primitive type of Marching Cubes
        public Type type;

        //The type of shading used for the visualisation
        public Shading shading;

        //The compute shader that executes the Marching Cubes algorithm
        private ComputeShader marchingCubesCS;
        private int marchingCubesCSKernel;

        /// <summary>
        /// Performs the Marching Cubes algorithm and generates the mesh.
        /// </summary>
        /// <param name="cells">The cells of a CellularAutomaton</param>
        override public void Render(ComputeBuffer cells)
        {
            mesh.SetCounterValue(0);
            marchingCubesCS.SetBuffer(marchingCubesCSKernel, "currentGeneration", cells);
            marchingCubesCS.Dispatch(marchingCubesCSKernel, dimensions.x, dimensions.y * 2, dimensions.z * 2);

            RenderTexture3D(cells);

            ComputeBuffer.CopyCount(mesh, args, 0);

#if REALISTIC
            if(!type.Equals(Type.CUBES))
                RenderRealisticWater();
#endif
        }


        /// <summary>
        /// Initializes the compute buffer, that stores the output of the Marching Cubes algorithm (triangles or quads).
        /// Also initializes the buffer, that stores the number of triangles or quads.
        /// </summary>
        override protected void InitializeComputeBuffer()
        {
            args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            data = new int[4] { 0, 1, 0, 0 };
            args.SetData(data);

            if (type.Equals(Type.CUBES))
            {
                mesh = new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, 4 * 3 * sizeof(float), ComputeBufferType.Append);
            }
            else
            {
                if (shading.Equals(Shading.FLAT))
                    mesh = new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, 3 * 3 * sizeof(float), ComputeBufferType.Append);
                else
                    mesh = new ComputeBuffer((dimensions.x * dimensions.y * dimensions.z) * 4096, 6 * 3 * sizeof(float), ComputeBufferType.Append);
            }

            ComputeBuffer.CopyCount(mesh, args, 0);
        }

        /// <summary>
        /// Loads the compute shader that computes the selected Marching Cubes algorithm.
        /// </summary>
        override protected void InitializeComputeShader()
        {
            string path = "MarchingCubes_";

            switch (type)
            {
                case Type.CUBES: path += "CUBES"; break;
                case Type.SIMPLE: path += "SIMPLE"; break;
                case Type.TESSELATION: path += "SIMPLE"; break;
                case Type.MULTIPLE_FLUIDS: path += "MULTIPLE"; break;
            }

            if (!type.Equals(Type.CUBES))
            {
                if (shading.Equals(Shading.GOURAUD) || shading.Equals(Shading.PHONG))
                {
                    path += "wNORMALS";
                }
            }

            marchingCubesCS = Resources.Load<ComputeShader>("ComputeShader/Marching Cubes/" + path);
            marchingCubesCSKernel = marchingCubesCS.FindKernel("CSMain");

            marchingCubesCS.SetInts("size", new int[] { dimensions.x * 16, dimensions.y * 16, dimensions.z * 16 });
            marchingCubesCS.SetBuffer(marchingCubesCSKernel, "mesh", mesh);
        }

        /// <summary>
        /// Initializes the material, that is used to render the output of the Marching Cubes algorithm.
        /// </summary>
        override protected void InitializeMaterial()
        {
            string path = "MC";

            switch (type)
            {
                case Type.CUBES: path += "_CUBES"; break;
                case Type.SIMPLE: path += "_"; break;
                case Type.TESSELATION: path += "_TESSELATION_"; break;
                case Type.MULTIPLE_FLUIDS: path += "_"; break;
            }

            if (!type.Equals(Type.CUBES))
            {
                switch (shading)
                {
                    case Shading.FLAT: path += "FLAT"; break;
                    case Shading.GOURAUD: path += "GOURAUD"; break;
                    case Shading.PHONG: path += "PHONG"; break;
                }
            }

            material = new Material(Resources.Load<Shader>("Shader/Marching Cubes/" + path));
        }


        /// <summary>
        /// Copy from the Water-basic Script from the standard assets.
        /// Used to render realistic water.
        /// </summary>
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

        /// <summary>
        /// Don't forget releasing the buffers.
        /// </summary>
        void OnDisable()
        {
            mesh.Release();
            args.Release();
        }
    }
}