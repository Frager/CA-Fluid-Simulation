using UnityEngine;
using GPUFluid;

public class FloatableManager : MonoBehaviour {

    public CellularAutomaton ca;

    public Vector3 gridPosition;
    private Vector3 cellSize;


    GameObject[] floatingGameObjects;
    IFloatable[] iFloatables;

    void Start ()
    {
        iFloatables = Object.FindObjectsOfType<IFloatable>();
        floatingGameObjects = new GameObject[iFloatables.Length];
        for (int i = 0; i < iFloatables.Length; i++)
        {
            floatingGameObjects[i] = iFloatables[i].gameObject;
        }
        ca.InitializeFloatableBuffer(iFloatables.Length);
        cellSize = ca.visualization.CellSize();
    }

    int timer = 30;
    int timerCount = 0;

    void Update()
    {
        if (timerCount++ > timer)
        {
            int[] coords = new int[iFloatables.Length * 3];
            float[] densities = new float[iFloatables.Length];
            for (int i = 0; i < iFloatables.Length; i++)
            {
                Vector3 floatablePosition = floatingGameObjects[i].transform.position - gridPosition;
                coords[i * 3] = (int)(floatablePosition.x / cellSize.x);
                coords[i * 3 + 1] = (int)(floatablePosition.y / cellSize.y);
                coords[i * 3 + 2] = (int)(floatablePosition.z / cellSize.z);
                densities[i] = iFloatables[i].density;
            }

            //ca.getFluidHeightsAtCoordinates(coords, densities); returns [forece x, force y, force z,floatHeight*]
            float[] waterlevels = ca.getFluidHeightsAtCoordinates(coords, densities);
            for (int i = 0; i < iFloatables.Length; i++)
            {
                //if(waterlevels[i * 4 + 3] > 0)
                    iFloatables[i].waterLevel = waterlevels[i * 4 + 3] * cellSize.y;
            }
            timerCount -= timer;
        }
        
    }
}
