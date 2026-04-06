using Pilot;
using Unity.Behavior;
using UnityEngine;

namespace Enemies.Leech {

    public class LeechCore : MonoBehaviour {

        private Rigidbody2D _rb;
        private Transform _ship;
        private float _lastBoostTime;
        
        public ResourceItem targetItem;
        
        [Tooltip("How much random offset to apply to the boost direction")]
        [SerializeField] private float boostSpread;

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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() { }

        // Update is called once per frame
        void Update() {
            
        }
        
        /// <summary>
        /// Periodically add force towards <paramref name="target"/>
        /// </summary>
        /// <param name="target">Location to move towards</param>
        public void JerkTowards(Vector3 target, float boostForce) {
            Vector2 dir = (target - transform.position).normalized;
            // Apply spread by multiplying dir vector by z-angle quaternion
            dir = Quaternion.Euler(0, 0, Random.Range(-boostSpread, boostSpread)) * dir;
            Rb.AddForce(dir * boostForce);
        }

    }

}