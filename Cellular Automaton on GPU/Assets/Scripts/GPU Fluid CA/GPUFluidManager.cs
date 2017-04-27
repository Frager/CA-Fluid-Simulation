using UnityEngine;

namespace GPUFLuid
{
    public class GPUFluidManager : MonoBehaviour
    {
        public CellularAutomaton ca;

        [Range(0.01f, 1f)]
        public float timeframe = 0.01f;

        public int x, y, z;

        private float timer = 0;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                ca.Fill(new int[] { x, y, z, 2 });
                ca.NextGeneration();

                timer -= timeframe;
            }
        }
    }
}

