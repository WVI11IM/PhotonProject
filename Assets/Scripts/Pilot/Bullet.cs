using System;
using UnityEngine;

namespace Pilot {

    public class Bullet : MonoBehaviour {

        [SerializeField] private float travelSpeed;

        private void FixedUpdate() {
            transform.Translate(transform.forward * Time.fixedDeltaTime);
        }

    }

}