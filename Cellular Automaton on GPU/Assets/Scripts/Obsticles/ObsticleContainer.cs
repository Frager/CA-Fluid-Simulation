using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsticleContainer : MonoBehaviour {

    public Vector3 gridPosition;
    public Vector3 cellSize;
    public int gridSize;
	
	void Start () {

        //get the transforms of all child objects
		foreach (Transform child in transform)
        {
            Vector3 scale = child.localScale;
            Vector3 position = child.position;
            
            position -= gridPosition;
            float corner = position.x - scale.x / 2f;
            int xStart = Mathf.RoundToInt(corner / cellSize.x);

            corner = position.x + scale.x / 2f;
            int xEnd = Mathf.RoundToInt(corner / cellSize.x);

            corner = position.y - scale.y / 2f;
            int yStart = Mathf.RoundToInt(corner / cellSize.y);

            corner = position.y + scale.y / 2f;
            int yEnd = Mathf.RoundToInt(corner / cellSize.y);

            corner = position.z - scale.z / 2f;
            int zStart = Mathf.RoundToInt(corner / cellSize.z);

            corner = position.z + scale.z / 2f;
            int zEnd = Mathf.RoundToInt(corner / cellSize.z);

            print(xStart);
            GameObject colider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            colider.name = "Colider";
            colider.transform.position = position;
            colider.transform.localScale = scale;

            child.gameObject.SetActive(false);
        }
	}

    private CornerCoords getCornerCoords(Vector3 position, Vector3 scale)
    {
        CornerCoords coords = new CornerCoords();
        return coords;
    }

	public struct CornerCoords
    {
        int xStart;
        int xEnd;
        int yStart;
        int yEnd;
        int zStart;
        int zEnd;
    }
}
