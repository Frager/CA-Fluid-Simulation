using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPUFluid;

public class FloatableManager : MonoBehaviour {

    public CellularAutomaton ca;

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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ca.getFluidHeightAtCoordinate(Vector3.one, 0f);
        }
        for (int i = 0; i < floatableComponents.Length; i++)
        {

        }
    }
}
