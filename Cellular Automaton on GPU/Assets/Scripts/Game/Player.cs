using UnityEngine;

[RequireComponent(typeof(IFloatable))]
public class Player : MonoBehaviour {

    private IFloatable floatable;

	void Start ()
    {
        floatable = GetComponent<IFloatable>();
	}
	
	void Update ()
    {
        floatable.density += Input.GetAxis("Mouse ScrollWheel");
        floatable.density = Mathf.Clamp(floatable.density, 0, 3);
	}
}
