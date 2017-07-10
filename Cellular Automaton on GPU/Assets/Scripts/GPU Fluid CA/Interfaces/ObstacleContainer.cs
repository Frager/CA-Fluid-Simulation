using UnityEngine;
using System.Collections.Generic;

public class ObstacleContainer : IObstacle
{
    public List<Transform> obstacles;
    private Vector3 gridPosition;
    private Vector3 cellSize;

    public override List<CornerCoords> getObstacleCorners(GPUFluid.GridDimensions dimensions, Vector3 _scale)
    {
        gridPosition = new Vector3(-(1.0f / dimensions.x), -(1.0f / dimensions.y), -(1.0f / dimensions.z));
        cellSize = new Vector3(_scale.x / (dimensions.x * 16.0f), _scale.y / (dimensions.y * 16.0f), _scale.z / (dimensions.z * 16.0f));

        foreach (Transform child in obstacles)
        {
            if (child.gameObject.activeSelf)
            {
                Vector3 scale = child.localScale;
                Vector3 position = child.position;

                position -= gridPosition;

                CornerCoords coords = getCornerCoords(position, scale, dimensions);
                cornerCoordList.Add(coords);

                //snaps Obstacle position and scale to grid
                child.position = Vector3.Scale(new Vector3((coords.xStart + coords.xEnd) / 2f,
                    (coords.yStart + coords.yEnd) / 2f,
                    (coords.zStart + coords.zEnd) / 2f), cellSize) + gridPosition;
                child.localScale = Vector3.Scale(new Vector3(coords.xEnd - coords.xStart,
                    coords.yEnd - coords.yStart,
                    coords.zEnd - coords.zStart), cellSize);
            }
        }
        return cornerCoordList;
    }

    private CornerCoords getCornerCoords(Vector3 position, Vector3 scale, GPUFluid.GridDimensions dimensions)
    {
        CornerCoords coords = new CornerCoords();

        float corner = position.x - scale.x / 2f;
        coords.xStart = Mathf.RoundToInt(Mathf.Clamp(corner / cellSize.x, 0, dimensions.x * 16));

        corner = position.x + scale.x / 2f;
        coords.xEnd = Mathf.RoundToInt(Mathf.Clamp(corner / cellSize.x, 0, dimensions.x * 16));

        corner = position.y - scale.y / 2f;
        coords.yStart = Mathf.RoundToInt(Mathf.Clamp(corner / cellSize.y, 0, dimensions.y * 16));

        corner = position.y + scale.y / 2f;
        coords.yEnd = Mathf.RoundToInt(Mathf.Clamp(corner / cellSize.y, 0, dimensions.y * 16));

        corner = position.z - scale.z / 2f;
        coords.zStart = Mathf.RoundToInt(Mathf.Clamp(corner / cellSize.z, 0, dimensions.z * 16));

        corner = position.z + scale.z / 2f;
        coords.zEnd = Mathf.RoundToInt(Mathf.Clamp(corner / cellSize.z, 0, dimensions.z * 16));

        return coords;
    }


}
