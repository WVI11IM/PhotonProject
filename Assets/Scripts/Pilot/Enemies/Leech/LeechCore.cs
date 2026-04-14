using System;
using Pilot;
using Pilot.Enemies;
using Pilot.Ship;
using Systems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies.Leech {
    
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ItemDropper))]
    public class LeechCore : Pooling<LeechCore>, IDamageable {

        private ItemDropper _dropper;
        private Rigidbody2D _rb;
        private Transform _ship;
        private int _maxHealth;
        private float _lastBoostTime;

        [SerializeField] private int health;
        public ResourceItem targetItem;
        [Tooltip("How much random offset to apply to the boost direction")]
        [SerializeField] private float boostSpread;

        [Header("Animation Config")]
        [SerializeField] private GameObject model;
        [SerializeField] private float rotationLerpSpeed;
        [SerializeField] private float spinPerVelocityMult;
        [SerializeField] private AnimationCurve squashAndStretchPerVelocity;

        public Rigidbody2D Rb {
            get {
                if (_rb == null)
                    _rb = GetComponent<Rigidbody2D>();
                return _rb;
            }
        }
        public Transform Ship {
            get {
                if (_ship == null)
                    _ship = FindAnyObjectByType<ShipControls>().transform;
                return _ship;
            }
        }

        private void Awake() {
            _dropper = GetComponent<ItemDropper>();
            _maxHealth = health;
        }

        // Update is called once per frame
        void Update() {
            float velMagnitude = Rb.linearVelocity.magnitude;
            float lerpTo = 0;
            if (transform.parent) {
                Vector2 parentDelta = transform.parent.position - transform.position;
                lerpTo = Mathf.Atan2(parentDelta.y, parentDelta.x);
            } else {
                lerpTo = Mathf.Atan2(Rb.linearVelocity.y, Rb.linearVelocity.x);
            }
            Rb.rotation = Mathf.LerpAngle(Rb.rotation, lerpTo * Mathf.Rad2Deg - 90, Time.deltaTime * rotationLerpSpeed);
            model.transform.Rotate(velMagnitude * spinPerVelocityMult * Time.deltaTime * Vector3.up);
            Vector3 scale = model.transform.localScale;
            scale.y = squashAndStretchPerVelocity.Evaluate(velMagnitude);
            model.transform.localScale = scale;
            if (targetItem)
                Debug.DrawLine(transform.position, targetItem.transform.position);
        }

        /// <summary>
        /// Periodically add force towards <paramref name="target"/>
        /// </summary>
        /// <param name="target">Location to move towards</param>
        /// <param name="boostForce">Force to apply in the specified direction</param>
        public void JerkTowards(Vector3 target, float boostForce) {
            Vector2 dir = (target - transform.position).normalized;
            // Apply spread by multiplying dir vector by z-angle quaternion
            dir = Quaternion.Euler(0, 0, Random.Range(-boostSpread, boostSpread)) * dir;
            Rb.AddForce(dir * boostForce);
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