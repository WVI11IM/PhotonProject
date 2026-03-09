using UnityEngine;

namespace Pilot {
    
    public class ShipWeapon : MonoBehaviour {

        [SerializeField] private ShipStats stats;

        [Header("Config")]
        [SerializeField] private int burstBulletCount;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() { }

        // Update is called once per frame
        void Update() { }


        void FireBurst() { }

        void FireStreamStart() { }

        void FireStreamEnd() { }

    }

}