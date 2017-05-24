using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floatable : MonoBehaviour {

    public float density;

    public float floatHeight = 2;
    public float bounceDamp = 0.05f;
    public Vector3 buoyancyCentreOffset;

    private float waterLevel = 1;
    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
    public void SetWaterLevel(float waterLevel)
    {
        this.waterLevel = waterLevel;
    }

    float forcefactor;
    Vector3 actionPoint;
    Vector3 upLift;

    void Update () {
        if (floatHeight > 0)
        {
            actionPoint = transform.position + transform.TransformDirection(buoyancyCentreOffset);
            forcefactor = 1f - ((actionPoint.y - waterLevel) / floatHeight);
            if (forcefactor > 0f)
            {
                upLift = -Physics.gravity * (forcefactor - rb.velocity.y * bounceDamp);
                rb.AddForceAtPosition(upLift, actionPoint);
            }
        }
	}
}
