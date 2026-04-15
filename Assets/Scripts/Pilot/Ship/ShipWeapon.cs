using Logitech;
using Systems;
using UnityEngine;

namespace Pilot.Ship {
    
    [AddComponentMenu("")]
    public class ShipWeapon : ShipComponent {

        [SerializeField] private ShipStats stats;

        [Header("Components")]
        [SerializeField] private Transform bulletSpawnPoint;

        [Header("Config")]
        [Range (0, 1)]
        [SerializeField] private float pedalDeadzone;
        [SerializeField] private AnimationCurve rateOfFire;
        [SerializeField] private AnimationCurve spread;
        [SerializeField] private AnimationCurve bulletTravelSpeed;
        [SerializeField] private AnimationCurve lifetime;

        private float _lastShotTime;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() { }

        // Update is called once per frame
        void Update() {

            if (LogitechUtil.AxisPedalClutch > pedalDeadzone && Time.time - _lastShotTime > 1 / rateOfFire.Evaluate(LogitechUtil.AxisPedalClutch)) {
                _lastShotTime = Time.time;
                FireSingleBullet(spread.Evaluate(LogitechUtil.AxisPedalClutch), bulletTravelSpeed.Evaluate(LogitechUtil.AxisPedalClutch), lifetime.Evaluate(LogitechUtil.AxisPedalClutch));
            }

        }

        private void FireSingleBullet(float sa, float bs, float lt) {
            if (Core.Stats.Ammo.Current <= 0)
                return;
            Core.Stats.Ammo.Consume(1);
            var b = Pooling<Bullet>.Retrieve(BulletType.Player, bs, lt);
            b.transform.position = bulletSpawnPoint.position;
            b.transform.rotation = bulletSpawnPoint.rotation *
                                   Quaternion.Euler(0, 0, Random.Range(-sa, sa));
        }

    }

}