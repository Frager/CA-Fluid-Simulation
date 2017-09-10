using UnityEngine;


namespace UnityStandardAssets.Vehicles.Ball
{
    [RequireComponent(typeof(Ball))]
    [RequireComponent(typeof(IFloatable))]
    public class Player : MonoBehaviour
    {

        private IFloatable floatable;
        private Ball ball;

        void Start()
        {
            floatable = GetComponent<IFloatable>();
            ball = GetComponent<Ball>();
        }

        void Update()
        {
            floatable.density += Input.GetAxis("Mouse ScrollWheel");
            floatable.density = Mathf.Clamp(floatable.density, 0, 3);
            if (floatable.isFloating())
            {
                ball.SetUseTorque(false);
            }
            else ball.SetUseTorque(true);
        }
    }
}