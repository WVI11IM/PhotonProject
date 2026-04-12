using System;
using Unity.Collections;
using UnityEngine;

namespace Pilot.Ship {

    [Serializable]
    public class ShipResource {
        
        [field:ReadOnly]
        [field:SerializeField] public float  Current { get; private set; }
        [field:SerializeField] public float  Max { get; private set; }
        [field:SerializeField] public float  RateReplenish { get; private set; }
        [field:SerializeField] public float  RatePenalty { get; private set; }

        public ShipResource(float max, float replenish, float penalty) {
            Current = max;
            Max = max;
        }
        public void Initialize() {
            Current = Max;
        }
        public void Replenish() {
            Current = Mathf.Clamp(Current + RateReplenish, 0, Max);
        }
        public void Penalty() {
            Current = Mathf.Clamp(Current - RatePenalty, 0, Max);
        }

        public void Consume(float amount) {
            if (amount < 0) {
                Debug.LogError($"Cannot consume negative amount({amount}), use Replenish() if you meant to add, or a positive value that will be subtracted!");
                return;
            }
            Current = Mathf.Clamp(Current - amount, 0, Max);
        }
        
        // Override cast to float, so value can be accessed directly.
        public static explicit operator float(ShipResource stat) => stat.Current;

    }
    
    [AddComponentMenu("")]
    public class ShipStats : ShipComponent {

        [Header("Resources")]
        [field:SerializeField] public ShipResource  Fuel { get; private set; }
        [field:SerializeField] public ShipResource  Ammo { get; private set; }
        [field:SerializeField] public ShipResource  Hull   { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            Fuel.Initialize();
            Ammo.Initialize();
            Hull.Initialize();
        }

        public void ReplenishResource(ItemType resource) => TypeToResource(resource).Replenish();
        public void IncorrectSectorPenalty(ItemType resource) => TypeToResource(resource).Penalty();

        public ShipResource TypeToResource(ItemType type) {
            // This could be done better, but it's good enough for now...
            switch (type) {
                case ItemType.Ammo:     return Ammo;
                case ItemType.Fuel:     return Fuel;
                case ItemType.Metal:    return Hull;
                default: return null;
            }
        }

    }

}