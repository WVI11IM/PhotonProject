using System;
using Systems;
using UnityEngine;

namespace Pilot {

    public enum BulletType {
        Player,
        Enemy
    }

    public class Bullet : Pooling<Bullet> {

        [SerializeField] private BulletType type;
        [SerializeField] private float travelSpeed;
        [SerializeField] private AnimationCurve travelSpeedFalloff;
        [SerializeField] private float lifetime;
        [SerializeField] private Material matPlayer;
        [SerializeField] private Material matEnemy;
        [SerializeField] private new MeshRenderer renderer;
        [SerializeField] private new TrailRenderer trailRenderer;
        [SerializeField] private Gradient trailGradientPlayer;
        [SerializeField] private Gradient trailGradientEnemy;

        private float _spawnTime;

        private void FixedUpdate() {
            if (!trailRenderer.emitting) {
                trailRenderer.emitting = true;
                trailRenderer.Clear();
            }
            transform.Translate(Vector2.up * (travelSpeed * travelSpeedFalloff.Evaluate((Time.time - _spawnTime) / lifetime) * Time.fixedDeltaTime));
            if (Time.time - _spawnTime >= lifetime)
                DeleteBullet();
        }

        private void DeleteBullet() {
            Stash();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            // Ignore collision if other object is neither Player, Enemy nor Obstacle
            if (!other.CompareTag("Player") && !other.CompareTag("Enemy") && !other.CompareTag("Obstacle"))
                return;
            // Ignore collision if bullet type does not match what it should do damage to
            if ((!other.CompareTag("Player") || type != BulletType.Enemy) &&
                (!other.CompareTag("Enemy") || type != BulletType.Player)) return;
            // Apply damage
            if (other.GetComponent<IDamageable>() != null)
                other.GetComponent<IDamageable>().TakeDamage();
            DeleteBullet();
        }

        protected override void Initialize(params object[] p) {
            gameObject.SetActive(true);
            _spawnTime = Time.time;
            type = (BulletType)p[0];
            renderer.material = type switch {
                BulletType.Enemy => matEnemy,
                BulletType.Player => matPlayer,
                _ => null
            };
            trailRenderer.colorGradient = type switch {
                BulletType.Enemy => trailGradientEnemy,
                BulletType.Player => trailGradientPlayer,
                _ => null
            };
        }
        protected override void Disable() {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
            gameObject.SetActive(false);
        }

    }

}