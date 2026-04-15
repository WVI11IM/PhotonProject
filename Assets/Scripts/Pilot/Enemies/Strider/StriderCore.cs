using System;
using Pilot;
using Pilot.Enemies;
using Pilot.Ship;
using Systems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies.Strider {

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ItemDropper))]
    public class StriderCore : Pooling<StriderCore>, IDamageable {

        private ItemDropper _dropper;
        private Rigidbody2D _rb;
        private int _maxHealth;

        [SerializeField] private int health;
        [SerializeField] private float rotateLerpSpeed;
        [SerializeField] private float bulletTravelSpeed = 10;
        [SerializeField] private float bulletLifetime = 1;

        private void Awake() {
            _dropper = GetComponent<ItemDropper>();
            _maxHealth = health;
        }

        public Rigidbody2D Rb {
            get {
                if (_rb == null)
                    _rb = GetComponent<Rigidbody2D>();
                return _rb;
            }
        }
        
        public void TorqueToFace(Vector2 target) {
            float rot = Mathf.Rad2Deg * Mathf.Atan2(target.y, target.x) - 90;
            rot = Mathf.LerpAngle(Rb.rotation, rot, Time.deltaTime * rotateLerpSpeed);
            Rb.MoveRotation(rot);
        }

        public void ShootVolley(int count, float spread) {
            for (var i = 0; i < count; i++) {
                var b = Pooling<Bullet>.Retrieve(BulletType.Enemy, bulletTravelSpeed, bulletLifetime);
                b.transform.position = transform.position;
                b.transform.rotation = transform.rotation *
                                       Quaternion.Euler(0, 0, Random.Range(-spread, spread));
            }
        }

        public void TakeDamage() {
            health--;
            if (health <= 0)
                Die();
        }

        private void Die() {
            _dropper.DropItems();
            Stash();
        }

        protected override void Initialize(params object[] p) {
            health = _maxHealth;
        }
        protected override void Disable() {
            gameObject.SetActive(false);
        }

    }

}
