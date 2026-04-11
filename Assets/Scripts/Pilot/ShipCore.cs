using Systems;
using UnityEngine;

namespace Pilot {

    [RequireComponent(typeof(ShipControls))]
    [RequireComponent(typeof(ShipResource))]
    [RequireComponent(typeof(ShipWeapon))]
    public class ShipCore : Systems.Singleton<ShipCore>, IDamageable {

        public ShipControls Controls    { get; private set; }
        public ShipStats    Stats       { get; private set; }
        public ShipTractor  Tractor     { get; private set; }
        public ShipWeapon   Weapon      { get; private set; }
        
        [Header("Config")]
        [Tooltip("How much of the hull resource to drain when receiving damage.")]
        [SerializeField] private int hullConsumptionPerHit;

        protected void Awake() {
            Controls = GetComponent<ShipControls>();
            Stats = GetComponent<ShipStats>();
            Tractor = GetComponentInChildren<ShipTractor>();
            Weapon = GetComponent<ShipWeapon>();
        }

        public void TakeDamage() {
            Stats.Hull.Consume(hullConsumptionPerHit);
        }

    }

}