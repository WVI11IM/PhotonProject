using System;
using Logitech;
using UnityEngine;

namespace Pilot.Ship {
    
    [AddComponentMenu("")]
    public class ShipControls : ShipComponent {

        [SerializeField] private ShipStats stats;

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

        private bool disabledSpring;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            
        }

        private void Update() {
            if (!disabledSpring)
                disabledSpring = LogitechUtil.StopSpringForce();
        }

        // Update is called once per frame
        void FixedUpdate() {
            
            Core.Rb.MoveRotation(-LogitechUtil.WheelAxisDegrees);

            if (Core.Stats.Fuel.Current <= 0)
                return;
            
            // Fuel factor is a number shared for both force calculation and fuel consumption during this frame
            float fuelFactor =
                LogitechUtil.AxisPedalAccelerator * // Use the accelerator...
                boostForceFuelFalloff.Evaluate(stats.Fuel.Current / stats.Fuel.Max) * // ...with falloff if fuel is low...
                Time.fixedDeltaTime;

            // Acceleration boost 
            Core.Rb.AddForce(
                boostForce * // ...plus the force multiplier...
                fuelFactor * // ...plues the fuel factor...
                transform.up); // ...to move in the direction the gun is pointing.
            
            // Fuel consumption
            Core.Stats.Fuel.Consume(fuelFactor * fuelConsumptionFactor);
            
        }

    }

}