using System.Collections.Generic;
using Logitech;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pilot.Ship {

    [AddComponentMenu("")]
    public class ShipTractor : ShipComponent {
        
        /// <summary>
        /// Contains items in range, without filtering for angle range
        /// </summary>
        private List<Collider2D> _objectsInRange = new();
        
        [field:SerializeField] public ContactFilter2D ItemsFilter {get; private set; }
        [field:SerializeField] public AnimationCurve RangeDistanceFalloff {get; private set; }
        [field:SerializeField] public AnimationCurve RangeAngleFalloff {get; private set; }
        [field:SerializeField] public float Factor { get; private set; }
        [field:SerializeField] public float Deadzone { get; private set; }
        [SerializeField] private float pullForce;
        public bool Attracting => Factor > Deadzone;

        // Update is called once per frame
        void Update() {
            Factor = LogitechUtil.AxisPedalBrake;
            if (Attracting)
                Attract();
        }
        
        /// <summary>
        /// Activates the tractor beam. Pulls in objects in range, using the script's Factor variable to determine the distance and angle range falloff
        /// </summary>
        void Attract() {

            Physics2D.OverlapCircle(transform.position, RangeDistanceFalloff.Evaluate(Factor), ItemsFilter, _objectsInRange);

            foreach (var coll in _objectsInRange) {
                float dot = Vector2.Dot((coll.transform.position - transform.position).normalized, transform.up);
                Debug.DrawLine(transform.position, coll.transform.position, dot > 1 - RangeAngleFalloff.Evaluate(Factor) ? Color.lawnGreen : Color.crimson);
                if (dot > 1 - RangeAngleFalloff.Evaluate(Factor)) {
                    Vector2 delta = transform.position - coll.transform.position;
                    coll.attachedRigidbody.AddForce(delta.normalized * DistanceFalloff(delta.magnitude, RangeDistanceFalloff.Evaluate(Factor), pullForce));
                }
            }
        }

        /// <summary>
        /// Performs quadratic...? falloff on <paramref name="dist"/> such that it returns 0 when <paramref name="dist"/> == <paramref name="maxDist"/>.
        /// Formula: max(0, (sqrt(<paramref name="maxDist"/>) - sqrt(<paramref name="dist"/>)) * <paramref name="intensity"/>).
        /// </summary>
        /// <param name="dist">Input distance</param>
        /// <param name="maxDist">Maximum distance. Return value approaches zero as <paramref name="dist"/> reaches this value</param>
        /// <param name="intensity">How much to increase return value as <paramref name="dist"/> approaches 0</param>
        /// <returns>Input with falloff applied</returns>
        float DistanceFalloff(float dist, float maxDist, float intensity) {
            return Mathf.Max(0, (Mathf.Sqrt(maxDist) - Mathf.Sqrt(dist)) * intensity);
        }

    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(ShipTractor))]
    public class ShipTractorGUI : Editor {

        private void OnSceneGUI() {
            ShipTractor instance = (ShipTractor)target;
            Handles.color = instance.Attracting ? Color.Lerp(Color.lawnGreen, Color.clear, 0.5f) : Color.Lerp(Color.crimson, Color.clear, 0.25f);
            float angle = instance.RangeAngleFalloff.Evaluate(instance.Factor) * 90;
            Handles.DrawSolidArc(instance.transform.position, instance.transform.forward, Quaternion.Euler(0, 0, -angle) * instance.transform.up, angle * 2, instance.RangeDistanceFalloff.Evaluate(instance.Factor));
        }

    }
    #endif

}
