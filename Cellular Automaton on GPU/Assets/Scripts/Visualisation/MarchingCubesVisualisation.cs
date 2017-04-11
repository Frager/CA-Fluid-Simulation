using UnityEngine;

public class MarchingCubesVisualisation : MonoBehaviour {

    public ComputeShader marchingCubesCS;
    public Material material;

    private ComputeBuffer vertices;

	void Start ()
    {
        vertices = new ComputeBuffer(3, 3 * sizeof(float), ComputeBufferType.GPUMemory);

        marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "vertices", vertices);
        marchingCubesCS.Dispatch(marchingCubesCS.FindKernel("CSMain"), 3, 1, 1);
    }


    private void OnPostRender()
    {
        material.SetPass(0);
        material.SetBuffer("vertices", vertices);

        Graphics.DrawProcedural(MeshTopology.Triangles, 3);
    }

    void OnDisable()
    {
        vertices.Release();
    }
}
