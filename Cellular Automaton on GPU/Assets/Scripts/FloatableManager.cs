using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPUFluid;

public class FloatableManager : MonoBehaviour {

    public CellularAutomaton ca;
    public Vector3 gridPosition;
    private Vector3 cellSize;

    GameObject[] floatableObjects;
    Floatable[] floatableComponents;

    // Use this for initialization
    void Start () {
        floatableComponents = Object.FindObjectsOfType<Floatable>();
        floatableObjects = new GameObject[floatableComponents.Length];
        for (int i = 0; i < floatableComponents.Length; i++)
        {
            floatableObjects[i] = floatableComponents[i].gameObject;
        }
	}

    int timer = 100;
    int timerCount = 0;
    void Update()
    {
        if (timerCount++ > timer)
        {
            for (int i = 0; i < floatableComponents.Length; i++)
            {
                int[] coord = new int[3];
                Vector3 position = floatableObjects[i].transform.position - gridPosition;
                coord[0] = (int)position.x;
                coord[1] = (int)position.y;
                coord[2] = (int)position.z;
                float floatHeight = ca.getFluidHeightAtCoordinate(coord, floatableComponents[i].Density);
                floatableComponents[i].floatHeight = floatHeight;
                print("Coord: "+ coord[0] + ", " + coord[1] + ", " + coord[2] + " floatHeight: " + floatHeight);
            }
            timerCount -= timer;
        }
        
    }
}
