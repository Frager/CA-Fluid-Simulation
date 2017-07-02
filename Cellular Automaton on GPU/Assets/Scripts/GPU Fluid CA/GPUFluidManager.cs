using UnityEngine;
using System.Collections.Generic;

namespace GPUFluid
{
    public enum Fluid
    {
        NONE, GAS, OIL, WATER
    }

    public class GPUFluidManager : MonoBehaviour
    {
        public CellularAutomaton ca;
        public IObstacle obstacles;
        public IObstacle removeObstacles;

        [Range(0.01f, 100f)]
        public float timeframe = 0.01f;

        [Range(0f, 1f)]
        public float x, y, z;

        public Fluid element;

        private float timer = 0;


        private void Start()
        {
            ca.Heat(new int[] { 5, 1, 5, 200 });
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

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                ca.Fill(new float[] { x, y, z }, (int)element - 1);
                ca.NextGeneration();
                timer -= timeframe;
            }
        }

        public void RemoveObstacles()
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

