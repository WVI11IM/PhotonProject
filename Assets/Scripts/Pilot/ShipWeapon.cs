using Logitech;
using UnityEngine;

namespace Pilot {
    
    public class ShipWeapon : MonoBehaviour {

        [SerializeField] private ShipStats stats;

        [Header("Components")]
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;

        // TODO: consider using pedal press depth to control stream/burst modes?
        [Header("Config")]
        [SerializeField] private float spreadAngle = 10;
        [Range (0, 1)]
        [SerializeField] private float pedalPressThreshold;
        [SerializeField] private int burstBulletCount;
        [SerializeField] private float streamChargeDuration;
        [SerializeField] private float streamDelayBetweenShots;

        private bool _firing;
        private float _streamStartTime;
        private float _streamLastShot;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() { }

        // Update is called once per frame
        void Update() {

            if (LogitechUtil.AxisPedalClutch > pedalPressThreshold) {
                if (!_firing)
                    FireStreamStart();
            } else if (_firing)
                FireStreamEnd();

            if (_firing && Time.time - _streamStartTime > streamChargeDuration) {
                if (Time.time - _streamLastShot > streamDelayBetweenShots) {
                    _streamLastShot = Time.time;
                    FireSingleBullet();
                }
            }
        
        }

        private void FireStreamStart() {
            _firing = true;
            _streamStartTime = Time.time;
        }

        private void FireStreamEnd() {
            _firing = false;
            if (Time.time - _streamStartTime < streamChargeDuration) {
                FireBurst();
            }
        }

        private void FireBurst() {
            for (int i = 0; i < burstBulletCount; i++)
                FireSingleBullet();
        }

        private void FireSingleBullet() {
            Bullet b = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation * Quaternion.Euler(0, 0, Random.Range(-spreadAngle, spreadAngle)));
        }

    }

}