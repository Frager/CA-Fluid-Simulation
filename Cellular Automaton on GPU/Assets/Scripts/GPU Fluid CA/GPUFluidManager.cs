using UnityEngine;
using System.Collections.Generic;

namespace GPUFLuid
{
    public class GPUFluidManager : MonoBehaviour
    {
        public CellularAutomaton ca;
        public ObstacleInterface obstacles;

        [Range(0.01f, 1f)]
        public float timeframe = 0.01f;

        [Range(0f, 1f)]
        public float x, y, z;

        [Range(0,2)]
        public int elementID = 2;

        private float timer = 0;


        private void Start()
        {
            ca.Heat(new int[] { 0, 0, 0, 0 });
            if (obstacles != null)
            {
                List<ObstacleInterface.CornerCoords> cornerList = obstacles.getObstacleCorners();
                if (cornerList != null)
                    foreach (ObstacleInterface.CornerCoords coords in cornerList)
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
                ca.Fill(new float[] { x, y, z }, elementID);
                ca.NextGeneration();

                timer -= timeframe;
            }
        }
    }
}

