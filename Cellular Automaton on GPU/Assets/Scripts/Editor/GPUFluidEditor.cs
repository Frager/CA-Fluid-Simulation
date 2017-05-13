
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GPUFluid.GPUFluidManager))]
public class GPUFluidEditor : Editor {

    public override void OnInspectorGUI()
    {
        GPUFluid.GPUFluidManager fm = (GPUFluid.GPUFluidManager)target;

        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Remove Obstacles"))
        {
            fm.RemoveObstacles();
        }
    }
}
