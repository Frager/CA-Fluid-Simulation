using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CellularAutomatonFinis))]
public class GPU_CA_Editor : Editor {


    public override void OnInspectorGUI()
    {
        CellularAutomatonFinis ca = (CellularAutomatonFinis)target;

        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Light Elements"))
        {
            ca.elementID = 1;
        }

        if (GUILayout.Button("Heavy Elements"))
        {
            ca.elementID = 2;
        }
    }
}
