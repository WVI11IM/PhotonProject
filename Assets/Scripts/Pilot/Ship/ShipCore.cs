using System;
using Misc;
using Systems;
using UnityEngine;

namespace Pilot.Ship {

    [RequireComponent(typeof(ShipControls))]
    [RequireComponent(typeof(ShipStats))]
    [RequireComponent(typeof(ShipTractor))]
    [RequireComponent(typeof(ShipWeapon))]
    public class ShipCore : Systems.Singleton<ShipCore>, IDamageable {

        public ShipControls Controls    { get; private set; }
        public ShipStats    Stats       { get; private set; }
        public ShipTractor  Tractor     { get; private set; }
        public ShipWeapon   Weapon      { get; private set; }

        private Rigidbody2D _rb;
        public Rigidbody2D Rb {
            get {

                if (_rb == null)
                    _rb = GetComponent<Rigidbody2D>();
                return _rb;
            }
        }
        
        [Header("Config")]
        [Tooltip("How much of the hull resource to drain when receiving damage.")]
        [SerializeField] private int hullConsumptionPerHit;
        [SerializeField] private float hitKnockback;

        protected void Awake() {
            Controls = GetComponent<ShipControls>();
            Stats = GetComponent<ShipStats>();
            Tractor = GetComponentInChildren<ShipTractor>();
            Weapon = GetComponent<ShipWeapon>();
        }

        public void TakeDamage(Bullet bullet) {
            Stats.Hull.Consume(hullConsumptionPerHit);
            if (Stats.Hull.Current <= 0)
                Die();
            Rb.AddForce((transform.position - bullet.transform.position).normalized * hitKnockback);
            Juice.Instance.InvokeHitFreeze();
            Juice.Instance.AddShake(0.5f);
        }

        public void OnCollisionEnter2D(Collision2D other) {
            Debug.Log("Touched KillScreen! Instakill!");
            if (other.collider.CompareTag("KillScreen"))
                Die();
        }

        public void Die() {
            Debug.Log("Player died!");
            gameObject.SetActive(false);
            Particles p = Pooling<Particles>.Retrieve(BulletType.Player, 0);
            p.transform.position = transform.position;
            p.transform.rotation = Quaternion.identity;
            GameOver.Show();
        }

    }

}