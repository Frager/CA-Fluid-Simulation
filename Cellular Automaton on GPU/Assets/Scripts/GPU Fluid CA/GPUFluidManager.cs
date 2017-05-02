using UnityEngine;

namespace GPUFLuid
{
    public class GPUFluidManager : MonoBehaviour
    {
        public CellularAutomaton ca;

        [Range(0.01f, 1f)]
        public float timeframe = 0.01f;

        public int x, y, z;

        [Range(0,2)]
        public int elementID = 2;

        private float timer = 0;

        private void Start()
        {
            ca.Heat(new int[] { 10, 1, 10, 120 });
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                ca.Fill(new int[] { x, y, z, elementID });
                ca.NextGeneration();

                timer -= timeframe;
            }
        }
    }
}

