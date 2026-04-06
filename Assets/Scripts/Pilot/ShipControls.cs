using UnityEngine;
using Logitech;

namespace Pilot {

    public enum SteeringMode {

        CLASSIC_VELOCITY,
        ABSOLUTE_ANGLE

    }
    
    public class ShipControls : MonoBehaviour {

        [SerializeField] private ShipStats stats;

        [SerializeField] private SteeringMode steeringMode;
        
        private Rigidbody2D _rb;

        [Header("Steering Config (Classic Velocity)")]
        [SerializeField] private float steerForce;
        [Header("Steering Config (Absolute Angle)")]
        [SerializeField] private float acceleration;
        [SerializeField] private float max;
        
        [Header("Boost Config")]
        [SerializeField] private float boostForce;
        // How much to reduce the boost force depending on how much fuel is left.
        [SerializeField] private AnimationCurve boostForceFuelFalloff;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void FixedUpdate() {
            if (steeringMode == SteeringMode.CLASSIC_VELOCITY) {
                // Steering rotation (apply torque according to steering wheel rotation)
                _rb.AddTorque(LogitechUtil.WheelAxis * -steerForce * Time.fixedDeltaTime);
                LogitechUtil.SetSpringForce(0, 1, 1);
            } else {
                float diff = (transform.rotation * Quaternion.Inverse(Quaternion.Euler(0, 0, 180 - LogitechUtil.WheelAxisDegrees))).eulerAngles.z -180;
                _rb.AddTorque(
                    Mathf.Clamp(diff * acceleration, -max, max) * Time.fixedDeltaTime);
                LogitechUtil.SetSpringForce(0, 0, 0);
            }

            // Acceleration boost 
            _rb.AddForce(
                LogitechUtil.AxisPedalAccelerator * // Use the accelerator...
                boostForce * // ...plus the force multiplier...
                boostForceFuelFalloff.Evaluate(stats.Fuel.Current / stats.Fuel.Max) * // ...with falloff if fuel is low...
                Time.fixedDeltaTime * // ...plus the delta time adjustment
                transform.up); // ...to move in the direction the gun is pointing.
        }

    }

}