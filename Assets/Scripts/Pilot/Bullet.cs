using System;
using Systems;
using UnityEngine;
using Random = UnityEngine.Random;

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
        [SerializeField] private float speedFuzz = 0.1f;
        [SerializeField] private Material matPlayer;
        [SerializeField] private Material matEnemy;
        [SerializeField] private new MeshRenderer renderer;
        [SerializeField] private new TrailRenderer trailRenderer;
        [SerializeField] private Gradient trailGradientPlayer;
        [SerializeField] private Gradient trailGradientEnemy;

        private float _spawnTime;
        private float _fuzzFactor;

        private void FixedUpdate() {
            if (!trailRenderer.emitting) {
                trailRenderer.emitting = true;
                trailRenderer.Clear();
            }

            float speed = travelSpeed * travelSpeedFalloff.Evaluate((Time.time - _spawnTime) / lifetime) * (1 + (_fuzzFactor * speedFuzz));
            transform.Translate(speed * Time.fixedDeltaTime * Vector2.up);
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
                other.GetComponent<IDamageable>().TakeDamage(this);
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
            if (p.Length > 1)
                travelSpeed = (float)p[1];
            if (p.Length > 2)
                lifetime = (float)p[2];
            _fuzzFactor = Random.value * 2 - 1;
        }
        protected override void Disable() {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
            gameObject.SetActive(false);
        }

    }

}