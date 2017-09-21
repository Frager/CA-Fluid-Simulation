using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
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

        public bool testRun;

        void Start()
        {
            SetObstacles();

            if (testRun)
            {
                x = 0;
                y = 0;
                z = 0;
                element = Fluid.WATER;
            }
        }

        private int counter = 0;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timeframe)
            {
                ca.Fill(new float[] { x, y, z }, (int)element - 1, radius);
                ca.NextGeneration();
                timer -= timeframe;
            }

            if (testRun)
            {
                if (counter < 1000)
                {
                    x += 0.001f;
                    y += 0.001f;
                    z += 0.001f;
                }
                else
                {
                    x -= 0.001f;
                    y -= 0.001f;
                    z -= 0.001f;
                }

                if (counter == 1000)
                    element = Fluid.OIL;

                if (counter == 2000)
                {
                    PrintMeasurements();
                    EditorApplication.isPlaying = false;
                }

                ++counter;
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

        public static void PrintMeasurements()
        {
            Debug.Log("Cellular Automaton: " + GetDataMean("Update.ScriptRunBehaviourUpdate/BehaviourUpdate/GPUFluidManager.Update()/CellularAutomaton.NextGeneration()/ComputeShader.Dispatch()"));
            Debug.Log("Marching Cubes: " + GetDataMean("Update.ScriptRunBehaviourUpdate/BehaviourUpdate/GPUFluidManager.Update()/CellularAutomaton.NextGeneration()/MarchingCubesVisualisation.Render()/ComputeShader.Dispatch()"));
        }

        public static float GetDataMean(string selectedPropertyPath = "")
        {
            var profilerSortColumn = ProfilerColumn.TotalTime;
            var viewType = ProfilerViewType.Hierarchy;
            var property = new ProfilerProperty();

            float meanValue = 0;
            int i = 0;

            for (int frameIndex = ProfilerDriver.firstFrameIndex; frameIndex <= ProfilerDriver.lastFrameIndex; ++frameIndex)
            {
                property.SetRoot(frameIndex, profilerSortColumn, viewType);
                property.onlyShowGPUSamples = false;

                const bool enterChildren = true;

                while (property.Next(enterChildren))
                {
                    bool shouldSaveProperty = string.IsNullOrEmpty(selectedPropertyPath) || property.propertyPath == selectedPropertyPath;
                    if (shouldSaveProperty)
                    {
                        meanValue += float.Parse(property.GetColumn(ProfilerColumn.TotalGPUTime));
                        ++i;
                    }
                }
                property.Cleanup();
            }

            meanValue /= i;

            return meanValue;
        }
    }
}

