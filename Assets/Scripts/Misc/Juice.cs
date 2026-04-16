using System.Collections;
using UnityEngine;

namespace Misc {

    public class Juice : Singleton<Juice> {

        private float time;
        private float shakeFactor;
        private bool hitFrozen;

        [Header("Screen Shake")]
        [SerializeField] private AnimationCurve shakeIntensityMult;
        [SerializeField] private float shakeSpeed;
        [SerializeField] private float shakeDecay;
        [Header("Hit Freeze")]
        [SerializeField] private float hitFreezeDuration = 0.1f;
        [Tooltip("Minimum amount of time between hitfreezes (prevents prolonged hitfreeze when hit by multiple bullets in quick succession).")]
        [SerializeField] private float hitFreezeGrace = 0.2f;

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

    }

}