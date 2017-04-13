using UnityEngine;

public class MarchingCubesVisualisation : MonoBehaviour {

    public ComputeShader marchingCubesCS;
    public Material material;

    private ComputeBuffer cells;
    private ComputeBuffer triangles;

	void Start ()
    {
        cells = new ComputeBuffer(16 * 16 * 16, 3 * sizeof(int));
        marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("Initialize"), "newGeneration", cells);
        marchingCubesCS.Dispatch(marchingCubesCS.FindKernel("Initialize"), 2, 2, 2);

        triangles = new ComputeBuffer(1024, 3 * 3 * sizeof(float), ComputeBufferType.Append);

        marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "triangles", triangles);
        marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "currentGeneration", cells);
        marchingCubesCS.Dispatch(marchingCubesCS.FindKernel("CSMain"), 2, 2, 2);
    }


    private void OnPostRender()
    {
        material.SetPass(0);
        material.SetBuffer("triangles", triangles);
        print(triangles.count);
        Graphics.DrawProcedural(MeshTopology.Triangles, triangles.count * 3);
    }

    void OnDisable()
    {
        triangles.Release();
    }
}
