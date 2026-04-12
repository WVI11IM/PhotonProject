using Pilot;
using Systems;
using UnityEngine;

namespace Enemies.Strider {

    [RequireComponent(typeof(Rigidbody2D))]
    public class StriderCore : MonoBehaviour, IDamageable {

        private Rigidbody2D _rb;
        private Transform _ship;

        [SerializeField] private int health;
        [Header("Movement Config")]
        [SerializeField] private float rotateForce;
        [SerializeField] private float rotateForceMax;

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
        
        public void TorqueToFace(Vector2 target) {
            float rot = -Mathf.Rad2Deg * Mathf.Atan2(target.y, target.x) + 90;
            float diff = (transform.rotation * Quaternion.Inverse(Quaternion.Euler(0, 0, 180 - rot))).eulerAngles.z -180;
            Rb.AddTorque(Mathf.Clamp(diff * -rotateForce, -rotateForceMax, rotateForceMax) * Time.fixedDeltaTime);
        }

        public void TakeDamage() {
            health--;
        }

        public void ShootVolley(int count, float spread) {
            for (var i = 0; i < count; i++) {
                var b = Pooling<Bullet>.Retrieve(BulletType.Enemy);
                b.transform.position = transform.position;
                b.transform.rotation = transform.rotation *
                                       Quaternion.Euler(0, 0, Random.Range(-spread, spread));
            }
        }

        private void Die() {
            
        }

    }

}
