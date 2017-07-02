
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GPUFluid.GPUFluidManager))]
public class GPUFluidEditor : Editor {

    private bool obsticlesRemoved = false;

    public override void OnInspectorGUI()
    {
        GPUFluid.GPUFluidManager fm = (GPUFluid.GPUFluidManager)target;

        if (DrawDefaultInspector())
        {

        }
        if (obsticlesRemoved)
        {
            if (GUILayout.Button("Set Obstacles"))
            {
                fm.SetObstacles();
                obsticlesRemoved = false;
            }
        }
        else if (GUILayout.Button("Remove Obstacles"))
        {
            fm.RemoveObstacles();
            obsticlesRemoved = true;
        }

    }
}
