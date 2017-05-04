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

            CornerCoords coords = getCornerCoords(position, scale);

            GameObject colider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            colider.name = "Colider";

            colider.transform.position = new Vector3((coords.xStart + coords.xEnd) / 2f,
                (coords.yStart + coords.yEnd) / 2f,
                (coords.zStart + coords.zEnd) / 2f);
            colider.transform.localScale = new Vector3(coords.xEnd - coords.xStart,
                coords.yEnd - coords.yStart,
                coords.zEnd - coords.zStart);

            child.gameObject.SetActive(false);
        }
	}

    private CornerCoords getCornerCoords(Vector3 position, Vector3 scale)
    {
        CornerCoords coords = new CornerCoords();

        float corner = position.x - scale.x / 2f;
        coords.xStart = Mathf.RoundToInt(corner / cellSize.x);

        corner = position.x + scale.x / 2f;
        coords.xEnd = Mathf.RoundToInt(corner / cellSize.x);

        corner = position.y - scale.y / 2f;
        coords.yStart = Mathf.RoundToInt(corner / cellSize.y);

        corner = position.y + scale.y / 2f;
        coords.yEnd = Mathf.RoundToInt(corner / cellSize.y);

        corner = position.z - scale.z / 2f;
        coords.zStart = Mathf.RoundToInt(corner / cellSize.z);

        corner = position.z + scale.z / 2f;
        coords.zEnd = Mathf.RoundToInt(corner / cellSize.z);

        return coords;
    }

	public struct CornerCoords
    {
        public int xStart;
        public int xEnd;
        public int yStart;
        public int yEnd;
        public int zStart;
        public int zEnd;
    }
}
