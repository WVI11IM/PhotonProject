using System.Collections;
using Logitech;
using UnityEngine;

namespace Misc {

    public class Juice : Systems.Singleton<Juice> {

        private float time;
        private float shakeFactor;
        private bool hitFrozen;
        private bool wheelJerking;
        private float wheelJerkIntensity;

        [Header("Screen Shake")]
        [SerializeField] private AnimationCurve shakeIntensityMult;
        [SerializeField] private float shakeSpeed;
        [SerializeField] private float shakeDecay;
        [Header("Hit Freeze")]
        [SerializeField] private float hitFreezeDuration = 0.1f;
        [Tooltip("Minimum amount of time between hitfreezes (prevents prolonged hitfreeze when hit by multiple bullets in quick succession).")]
        [SerializeField] private float hitFreezeGrace = 0.2f;
        [Header("Logitech")]
        [SerializeField] private float wheelJerkDuration = 0.25f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
        }

        // Update is called once per frame
        void Update() {

            time += Time.unscaledDeltaTime;

            transform.localPosition = new Vector2(
                Mathf.PerlinNoise(Mathf.PI, time * shakeSpeed),
                Mathf.PerlinNoise(Mathf.PI, time * shakeSpeed)
            ) * shakeIntensityMult.Evaluate(shakeFactor);

            shakeFactor -= shakeDecay * Time.deltaTime;
            shakeFactor = Mathf.Clamp01(shakeFactor);

        }

        public void AddShake(float amount) {
            shakeFactor += amount;
        }

        public void InvokeHitFreeze() {
            if (hitFrozen)
                return;
            StartCoroutine(nameof(HitFreezeCoroutine));
        }

        private IEnumerator HitFreezeCoroutine() {
            hitFrozen = true;
            float defaultTimeScale = Time.timeScale;
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(hitFreezeDuration);
            Time.timeScale = defaultTimeScale;
            yield return new WaitForSecondsRealtime(hitFreezeGrace);
            hitFrozen = false;
        }

        public void InvokeLogiWheelJerk(float intensity) {
            if (wheelJerking)
                return;
            wheelJerkIntensity = intensity;
            StartCoroutine(nameof(WheelJerkCoroutine));
        }

        private IEnumerator WheelJerkCoroutine() {
            wheelJerking = true;
            LogitechUtil.SetSpringForce(Random.Range(-1f, 1f), wheelJerkIntensity, 1);
            yield return new WaitForSecondsRealtime(hitFreezeDuration);
            LogitechUtil.StopSpringForce();
            wheelJerking = false;
        }



    }

}