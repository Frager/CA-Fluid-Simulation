using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Button : MonoBehaviour {

    public float threshold;
    public Material activatedMaterial, deactivatedMaterial;

    public GPUFluid.Fluid type;
    private GPUFluid.GPUFluidManager fluidManager;

    private bool activated = false;

    void Start()
    {
        fluidManager = FindObjectOfType<GPUFluid.GPUFluidManager>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag.Equals("Player"))
        {
            if(collision.relativeVelocity.magnitude > threshold)
            {
                Activate();
            }
        }
    }

    private void Activate()
    {
        if(!activated)
        {
            activated = true;
            fluidManager.element = type;
            GetComponent<Renderer>().material = activatedMaterial;
        }
        else
        {
            activated = false;
            fluidManager.element = GPUFluid.Fluid.NONE;
            GetComponent<Renderer>().material = deactivatedMaterial;
        }
    }
}
