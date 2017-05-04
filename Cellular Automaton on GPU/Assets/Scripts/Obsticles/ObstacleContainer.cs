using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleContainer : ObstacleInterface
{

    public Vector3 gridPosition;
    public Vector3 cellSize;
    public int gridSize;

    public override List<CornerCoords> getObstacleCorners()
    {
        foreach (Transform child in transform)
        {
            Vector3 scale = child.localScale;
            Vector3 position = child.position;

            position -= gridPosition;

            CornerCoords coords = getCornerCoords(position, scale);
            cornerCoordList.Add(coords);

            //snaps Obstacle position and scale to grid
            child.position = Vector3.Scale(new Vector3((coords.xStart + coords.xEnd) / 2f,
                (coords.yStart + coords.yEnd) / 2f,
                (coords.zStart + coords.zEnd) / 2f), cellSize);
            child.localScale = Vector3.Scale(new Vector3(coords.xEnd - coords.xStart,
                coords.yEnd - coords.yStart,
                coords.zEnd - coords.zStart), cellSize);
        }
        return cornerCoordList;
    }

 //   void Start () {
 //       //get the transforms of all child objects
	//	foreach (Transform child in transform)
 //       {
 //           Vector3 scale = child.localScale;
 //           Vector3 position = child.position;
            
 //           position -= gridPosition;

 //           CornerCoords coords = getCornerCoords(position, scale);
 //           cornerCoordList.Add(coords);

 //           //snaps Obstacle position and scale to grid
 //           child.position = Vector3.Scale(new Vector3((coords.xStart + coords.xEnd) / 2f,
 //               (coords.yStart + coords.yEnd) / 2f,
 //               (coords.zStart + coords.zEnd) / 2f), cellSize);
 //           child.localScale = Vector3.Scale(new Vector3(coords.xEnd - coords.xStart,
 //               coords.yEnd - coords.yStart,
 //               coords.zEnd - coords.zStart), cellSize);
 //       }
	//}

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


}
