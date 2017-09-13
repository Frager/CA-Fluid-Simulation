using UnityEngine;
using System.Collections.Generic;

namespace GPUFluid
{
    //Enumeration that stores the available fluids for the cellular automaton.
    //If you add something here you also have to update the ComputeShader.
    public enum Fluid
    {
        NONE, GAS, OIL, WATER
    }

    /// <summary>
    /// This class is responsible for controlling the cellular automaton. 
    /// </summary>
    public class GPUFluidManager : MonoBehaviour
    {
        public CellularAutomaton ca;

        public IObstacle obstacles;
        public IObstacle removeObstacles;

        //The update-rate of the cellular automaton. Number of updates per second = 1 / timeframe.
        [Range(0.01f, 100f)]
        public float timeframe = 0.01f;

        private float timer = 0;

        //The coordinates, where fluid is filled in.
        [Range(0f, 1f)]
        public float x, y, z;

        [Range(1, 3)]
        public int radius;

        //The type of fluid, that is filled in.
        public Fluid element;

        void Start()
        {
            SetObstacles();
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                ca.Fill(new float[] { x, y, z }, (int)element - 1, radius);
                ca.NextGeneration();
                timer -= timeframe;
            }
        }

        public void SetObstacles()
        {
            if (obstacles != null)
            {
                List<IObstacle.CornerCoords> cornerList = obstacles.getObstacleCorners(GetComponent<CellularAutomaton>().dimensions, ca.visualization.scale);
                if (cornerList != null)
                    foreach (IObstacle.CornerCoords coords in cornerList)
                    {
                        int[] start = { coords.xStart, coords.yStart, coords.zStart };
                        int[] end = { coords.xEnd, coords.yEnd, coords.zEnd };
                        ca.SetObstacle(start, end);
                    }
            }
        }

        public void RemoveObstacles()
        {
            if (removeObstacles != null)
            {
                List<IObstacle.CornerCoords> cornerList = removeObstacles.getObstacleCorners(GetComponent<CellularAutomaton>().dimensions, ca.visualization.scale);
                if (cornerList != null)
                    foreach (IObstacle.CornerCoords coords in cornerList)
                    {
                        int[] start = { coords.xStart, coords.yStart, coords.zStart };
                        int[] end = { coords.xEnd, coords.yEnd, coords.zEnd };
                        ca.RemoveObstacle(start, end);
                    }
            }
        }
    }
}

