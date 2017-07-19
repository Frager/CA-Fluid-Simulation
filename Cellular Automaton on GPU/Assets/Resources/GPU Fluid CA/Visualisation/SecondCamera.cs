using UnityEngine;

public class SecondCamera : MonoBehaviour {

    public Material material;
    public ComputeBuffer args;

    void OnPostRender()
    {
        material.SetPass(0);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, args);
    }
}
