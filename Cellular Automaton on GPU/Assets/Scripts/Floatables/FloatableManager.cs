using UnityEngine;
using GPUFluid;

public class FloatableManager : MonoBehaviour {

    public CellularAutomaton ca;

    public Vector3 gridPosition;
    private Vector3 cellSize;


    GameObject[] floatableObjects;
    Floatable[] floatableComponents;

    void Start ()
    {
        floatableComponents = Object.FindObjectsOfType<Floatable>();
        floatableObjects = new GameObject[floatableComponents.Length];
        for (int i = 0; i < floatableComponents.Length; i++)
        {
            floatableObjects[i] = floatableComponents[i].gameObject;
        }
        ca.InitializeFloatableBuffer(floatableComponents.Length);
        cellSize = ca.visualization.CellSize();
    }

    int timer = 30;
    int timerCount = 0;

    void Update()
    {
        if (timerCount++ > timer)
        {
            int[] coords = new int[floatableComponents.Length * 3];
            float[] densities = new float[floatableComponents.Length];
            for (int i = 0; i < floatableComponents.Length; i++)
            {
                Vector3 position = floatableObjects[i].transform.position - gridPosition;
                coords[i * 3] = (int)(position.x / cellSize.x);
                coords[i * 3 + 1] = (int)(position.y / cellSize.y);
                coords[i * 3 + 2] = (int)(position.z / cellSize.z);
                densities[i] = floatableComponents[i].density;
            }

            //ca.getFluidHeightsAtCoordinates(coords, densities); returns [forece x, force y, force z,floatHeight*]
            float[] waterlevels = ca.getFluidHeightsAtCoordinates(coords, densities);
            for (int i = 0; i < floatableComponents.Length; i++)
            {
                //if(waterlevels[i * 4 + 3] > 0)
                    floatableComponents[i].waterLevel = waterlevels[i * 4 + 3] * cellSize.y;
            }
            timerCount -= timer;
        }
        
    }
}
