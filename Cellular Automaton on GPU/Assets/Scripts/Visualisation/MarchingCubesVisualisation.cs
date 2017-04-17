using UnityEngine;

public class MarchingCubesVisualisation : MonoBehaviour {

    public int size = 16;

    public ComputeShader marchingCubesCS;
    public Material material;

    private ComputeBuffer cells;
    private ComputeBuffer triangles;
    private ComputeBuffer args;
    private int[] data;

	void Start ()
    {
        args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        data = new int[4] { 0, 1, 0, 0 };
        args.SetData(data);

        cells = new ComputeBuffer(size * size * size, 3 * sizeof(int));
        marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("Initialize"), "newGeneration", cells);
        marchingCubesCS.Dispatch(marchingCubesCS.FindKernel("Initialize"), size / 8, size / 8, size / 8);

        triangles = new ComputeBuffer(size * size * size, 3 * 3 * sizeof(float), ComputeBufferType.Append);

        marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "triangles", triangles);
        marchingCubesCS.SetBuffer(marchingCubesCS.FindKernel("CSMain"), "currentGeneration", cells);
        marchingCubesCS.Dispatch(marchingCubesCS.FindKernel("CSMain"), size / 8, size / 8, size / 8);

        ComputeBuffer.CopyCount(triangles, args, 0);
        args.GetData(data);
    }


    private void OnPostRender()
    {
        material.SetPass(0);
        material.SetBuffer("triangles", triangles);
        print(data[0]);
        Graphics.DrawProcedural(MeshTopology.Triangles, data[0] * 3);
    }

    void OnDisable()
    {
        triangles.Release();
        cells.Release();
        args.Release();
    }
}
