using Pilot.Ship;
using UnityEngine;

namespace Pilot {

    public class CameraTracking : MonoBehaviour {

        [SerializeField] private float minSpeed;
        [SerializeField] private float maxSpeed;
        [Tooltip("How much velocity to apply relative to the distance between the camera and the ship")]
        [SerializeField] private float catchupDifferential;
        [Tooltip("Start picking up the pace if the ship if this much farther along than the camera (relative to the camera's current y-position)")]
        [SerializeField] private float baseline;
        [SerializeField] private float lerpSpeed;

        private Vector3 _targetPos;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _targetPos = transform.position;
        }

        // Update is called once per frame
        void FixedUpdate() {
            float speed =
                Mathf.Clamp(
                    (ShipCore.Instance.transform.position.y - _targetPos.y + baseline) * catchupDifferential, 
                    minSpeed,
                    maxSpeed
                );
            _targetPos += speed * Time.fixedDeltaTime * Vector3.up;
            transform.position = Vector3.Lerp(transform.position, _targetPos, Time.fixedDeltaTime * lerpSpeed);
        }

    }

}