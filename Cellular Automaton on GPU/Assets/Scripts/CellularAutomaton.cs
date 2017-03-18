using UnityEngine;

public class CellularAutomaton : MonoBehaviour
{
    public ComputeShader computeShader;

    public ComputeShader CA2Texture3D;

    public Material testMaterial;

    private ComputeBuffer cellBuffer;

    private const int size = 8;

    private Cell[] cells;

    private RenderTexture texture3D;

    void Start ()
    {
        cells = new Cell[size * size * size];

        cellBuffer = new ComputeBuffer(cells.Length, 4 * sizeof(int), ComputeBufferType.GPUMemory);

        texture3D = new RenderTexture(size, size, 1);
        texture3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        texture3D.volumeDepth = size;
        texture3D.enableRandomWrite = true;
        texture3D.Create();
        testMaterial.SetTexture("_MainTex", texture3D);

        StartComputeShader();
        for(int i = 0; i < size; ++i)
            for (int j = 0; j < size; ++j)
                FillComputeShader(new Vector4(i, size - 1, j, 1));

        PrintCell(new Vector3(0, size - 1, 0));
        PrintCell(new Vector3(0, 0, 0));
    }

    void Update ()
    {
        UpdateComputeShader();
        Render();
    }

    void OnApplicationQuit()
    {
        PrintCell(new Vector3(0, size - 1, 0));
        PrintCell(new Vector3(0, 0, 0));
        cellBuffer.Release();
    }

    void StartComputeShader()
    {
        int kernelHandle = computeShader.FindKernel("Initialize");

        computeShader.SetBuffer(kernelHandle, "Cells", cellBuffer);

        computeShader.SetFloat("size", size);

        computeShader.Dispatch(kernelHandle, size / 4, size / 4, size / 4);
    }

    void FillComputeShader(Vector4 vector)
    {
        int kernelHandle = computeShader.FindKernel("Fill");

        computeShader.SetBuffer(kernelHandle, "Cells", cellBuffer);

        computeShader.SetVector("fill", vector);

        computeShader.SetFloat("size", size);

        computeShader.Dispatch(kernelHandle, 1, 1, 1);
    }

    void Render()
    {
        int kernelHandle = CA2Texture3D.FindKernel("CSMain");

        CA2Texture3D.SetBuffer(kernelHandle, "Cells", cellBuffer);

        CA2Texture3D.SetTexture(kernelHandle, "Result", texture3D);

        CA2Texture3D.SetFloat("size", size);

        CA2Texture3D.Dispatch(kernelHandle, size / 4, size / 4, size / 4);
    }

    void UpdateComputeShader()
    {
        int kernelHandle = computeShader.FindKernel("Update");

        computeShader.SetBuffer(kernelHandle, "Cells", cellBuffer);

        computeShader.SetFloat("size", size);

        computeShader.Dispatch(kernelHandle, size / 4, size / 4, size / 4);
    }

    void PrintCell(Vector3 position)
    {
        cellBuffer.GetData(cells);

        int index = (int)(position.x + position.y * size + position.z * size * size);
        print(cells[index].x + "," + cells[index].y + "," + cells[index].z + " --- " + cells[index].content);
    }

    void PrintCells()
    {
        cellBuffer.GetData(cells);

        for (int i = 0; i < cells.Length; ++i)
            print(cells[i].x + "," + cells[i].y + "," + cells[i].z + " --- " + cells[i].content);
    }

    struct Cell
    {
        public int x, y, z, content;
    }
}
