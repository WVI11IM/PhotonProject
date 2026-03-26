using System;
using UnityEngine;

namespace Pilot {

    public class Bullet : MonoBehaviour {

        [SerializeField] private float travelSpeed;

        private void FixedUpdate() {
            transform.Translate(Vector2.up * (travelSpeed * Time.fixedDeltaTime));
        }

    }

}