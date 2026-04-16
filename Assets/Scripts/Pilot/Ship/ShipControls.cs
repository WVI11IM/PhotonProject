using Logitech;
using UnityEngine;

namespace Pilot.Ship {

    public enum SteeringMode {

        CLASSIC_VELOCITY,
        ABSOLUTE_ANGLE

    }
    
    [AddComponentMenu("")]
    public class ShipControls : ShipComponent {

        [SerializeField] private ShipStats stats;

        [SerializeField] private SteeringMode steeringMode;
        
        private Rigidbody2D _rb;

        [SerializeField] private float fuelConsumptionFactor;

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
                _rb.MoveRotation(-LogitechUtil.WheelAxisDegrees);
                LogitechUtil.SetSpringForce(0, 0, 0);
            }

            if (Core.Stats.Fuel.Current <= 0)
                return;
            
            // Fuel factor is a number shared for both force calculation and fuel consumption during this frame
            float fuelFactor =
                LogitechUtil.AxisPedalAccelerator * // Use the accelerator...
                boostForceFuelFalloff.Evaluate(stats.Fuel.Current / stats.Fuel.Max) * // ...with falloff if fuel is low...
                Time.fixedDeltaTime;

            // Acceleration boost 
            _rb.AddForce(
                boostForce * // ...plus the force multiplier...
                fuelFactor * // ...plues the fuel factor...
                transform.up); // ...to move in the direction the gun is pointing.
            
            // Fuel consumption
            Core.Stats.Fuel.Consume(fuelFactor * fuelConsumptionFactor);
            
        }

    }

}