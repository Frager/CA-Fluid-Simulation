using UnityEngine;

public class CellularAutomatonFinis : MonoBehaviour {

    public ComputeShader computeShader;

    public ComputeShader CA2Texture3D;

    public Material testMaterial;

    private RenderTexture cellBuffer1;
    private RenderTexture cellBuffer2;

    private int buffer = 1;
    private int offsetSwitch = 0;

    public static int size = 8;

    [Range(1, 8)]
    public int elementID = 2;
    
    private RenderTexture texture3D;

    public SimpleVisuals visuals;

    void Start()
    {
        cellBuffer1 = new RenderTexture(size, size, 1, RenderTextureFormat.ARGBInt);
        cellBuffer1.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        cellBuffer1.volumeDepth = size;
        cellBuffer1.enableRandomWrite = true;
        cellBuffer1.Create();

        cellBuffer2 = new RenderTexture(size, size, 1, RenderTextureFormat.ARGBInt);
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

        InitializeBuffers();
        visuals.GenerateVisuals(transform.position, size, size, size, testMaterial);
        Render();
        InvokeRepeating("NextGeneration", 0, 0.1f);
    }

    void InitializeBuffers()
    {
        int kernelHandle = computeShader.FindKernel("Initialize");

        computeShader.SetInts("fill", new int[] { size - 1, size - 1, size - 1, 0 });

        computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer2);

        computeShader.Dispatch(kernelHandle, size / 8, size / 8, size / 8);

        computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer1);

        computeShader.Dispatch(kernelHandle, size / 8, size / 8, size / 8);
    }

    int counter = 0;

    void NextGeneration()
    {
        UpdateMethod();
        Render();
        buffer = (buffer == 1) ? 2 : 1;
        offsetSwitch = (offsetSwitch + 1) % 6;
    }

    void Render()
    {
        int kernelHandle = CA2Texture3D.FindKernel("CSMain");

        if (buffer == 1)
            CA2Texture3D.SetTexture(kernelHandle, "Cells", cellBuffer1);
        else
            CA2Texture3D.SetTexture(kernelHandle, "Cells", cellBuffer2);

        CA2Texture3D.SetTexture(kernelHandle, "Result", texture3D);

        CA2Texture3D.Dispatch(kernelHandle, size / 8, size / 8, size / 8);
    }

    void UpdateMethod()
    {
        int kernelHandle = computeShader.FindKernel("NextGeneration");

        switch(offsetSwitch)
        {
            case 0:
                computeShader.SetInt("offsetX", 0);
                computeShader.SetInt("offsetY", 0);
                computeShader.SetInt("offsetZ", 0);
                computeShader.SetInts("order", new int[] { 0, 1, 2, 3 });
                break;
            case 1:
                computeShader.SetInt("offsetX", 0);
                computeShader.SetInt("offsetY", 1);
                computeShader.SetInt("offsetZ", 0);
                computeShader.SetInts("order", new int[] { 0, 1, 2, 3 });
                break;
            case 2:
                computeShader.SetInt("offsetX", 1);
                computeShader.SetInt("offsetY", 0);
                computeShader.SetInt("offsetZ", 0);
                computeShader.SetInts("order", new int[] { 1, 0, 2, 3 });
                break;
            case 3:
                computeShader.SetInt("offsetX", 0);
                computeShader.SetInt("offsetY", 1);
                computeShader.SetInt("offsetZ", 0);
                computeShader.SetInts("order", new int[] { 1, 0, 2, 3 });
                break;
            case 4:
                computeShader.SetInt("offsetX", 0);
                computeShader.SetInt("offsetY", 0);
                computeShader.SetInt("offsetZ", 1);
                computeShader.SetInts("order", new int[] { 3, 2, 1, 0 });
                break;
            case 5:
                computeShader.SetInt("offsetX", 0);
                computeShader.SetInt("offsetY", 1);
                computeShader.SetInt("offsetZ", 0);
                computeShader.SetInts("order", new int[] { 3, 2, 1, 0 });
                break;
        }

        if (buffer == 1)
        {
            //if(counter < 3)
                computeShader.SetInts("fill", new int[] { 2, 3, 2, elementID });
            //else
             //   computeShader.SetInts("fill", new int[] { 4, 4, 4, elementID });
            //print(++counter);
            computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer1);
            computeShader.SetTexture(kernelHandle, "OldCells", cellBuffer2);
        }
        else
        {
            computeShader.SetTexture(kernelHandle, "NewCells", cellBuffer2);
            computeShader.SetTexture(kernelHandle, "OldCells", cellBuffer1);
        }

        computeShader.Dispatch(kernelHandle, size / 8, size / 8, size / 8);
    }
}
