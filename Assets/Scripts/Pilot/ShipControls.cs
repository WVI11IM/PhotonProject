using UnityEngine;
using Logitech;

namespace Pilot {

    public class ShipControls : MonoBehaviour {
        
        [Header("Components")]
        private Rigidbody2D _rb;
        [SerializeField] private GameObject gun;

        [Header("Config")]
        [SerializeField] private float steerForce;
        [SerializeField] private float boostForce;
        // How much to reduce the boost force depending on how much fuel is left.
        [SerializeField] private AnimationCurve boostForceFuelFalloff;

        [Header("Resources (Max)")]
        [SerializeField] private float  resMaxFuel;
        [SerializeField] private int    resMaxAmmo;
        [SerializeField] private int    resMaxHp;
        
        [Header("Resources (Current)")]
        [SerializeField] private float  resFuel;
        [SerializeField] private int    resAmmo;
        [SerializeField] private int    resHp;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void FixedUpdate() {
            // Steering rotation (apply torque according to steering wheel rotation)
            _rb.AddTorque(LogitechUtil.WheelAxis * steerForce * Time.fixedDeltaTime);
            // Acceleration boost 
            _rb.AddForce(
                LogitechUtil.AxisPedalAccelerator * // Use the accelerator...
                boostForce * // ...plus the force multiplier...
                boostForceFuelFalloff.Evaluate(resFuel / resMaxFuel) * // ...with falloff if fuel is low...
                Time.fixedDeltaTime * // ...plus the delta time adjustment
                gun.transform.forward); // ...to move in the direction the gun is pointing.
        }

    }

}