using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleVisuals : MonoBehaviour {
    
    //public Texture3D visuals;
    public Material material;
    [Range(0.1f,1f)]
    public float sphereSize;

    public void GenerateVisuals(Vector3 position, float xDim, float yDim, float zDim)
    {
        float halfPixelX = 1f / (xDim * 2f);
        float halfPixelY = 1f / (yDim * 2f);
        float halfPixelZ = 1f / (zDim * 2f);
        GameObject parent = new GameObject("Visuals");
        parent.transform.position = position;
        for (int z = 0; z < xDim; z++)
        {
            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.SetParent(parent.transform);
                    sphere.transform.position = new Vector3(x, y, z);
                    sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
                    sphere.GetComponent<Renderer>().material = material;
                    sphere.GetComponent<Renderer>().material.SetVector("_UVCoord", new Vector4(
                        (float)x / xDim + halfPixelX,
                        (float)y / yDim + halfPixelY,
                        (float)z / zDim + halfPixelZ,
                        1f
                        ));
                }
            }
        }
    }
}
