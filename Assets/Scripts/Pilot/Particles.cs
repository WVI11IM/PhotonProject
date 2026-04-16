using System;
using Systems;
using Unity.VisualScripting;
using UnityEngine;

namespace Pilot {

    public class Particles : Pooling<Particles> {

        [SerializeField] private Material matPlayer;
        [SerializeField] private Material matEnemy;

        [SerializeField] private ParticleSystem[] systems;
        private ParticleSystemRenderer[] renderers;

        public ParticleSystem ActiveSystem => activeSystemIndex == -1 ? null : systems[activeSystemIndex];

        private int activeSystemIndex = -1;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void InitializeRenderers() {
            renderers = new ParticleSystemRenderer[systems.Length];

            for (int i = 0; i < systems.Length; i++) {
                systems[i].Stop();
                ParticleSystem.MainModule main = systems[i].main;
                main.playOnAwake = false;
                renderers[i] = systems[i].GetComponent<ParticleSystemRenderer>();
                systems[i].gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update() {
            // If the system that was activated finished playing, 
            if (!systems[activeSystemIndex].isPlaying)
                Stash();
        }

        protected override void Initialize(params object[] p) {
            if (p.Length < 2 || p[0] is not BulletType || p[1] is not int)
                Debug.LogError($"{typeof(Particles)} was not initialized with the correct set of parameters");
            if (renderers == null || renderers.Length != systems.Length)
                InitializeRenderers();
            activeSystemIndex = (int)p[1];
            for (int i = 0; i < systems.Length; i++) {
                if (i == activeSystemIndex) {
                    if (!renderers[i])
                        Debug.LogError(
                            $"Could not find ParticleSystemRenderer associated with {renderers[i].gameObject.name}!",
                            renderers[i]);
                    else
                        renderers[i].material = (BulletType)p[0] switch {
                            BulletType.Player => matPlayer,
                            BulletType.Enemy => matEnemy,
                            _ => null
                        };
                    systems[i].gameObject.SetActive(true);
                    systems[i].Stop();
                    systems[i].Play();
                } else {
                    systems[i].Stop();
                    systems[i].gameObject.SetActive(false);
                }
            }
        }
        
        protected override void Disable() {
            foreach (var t in systems) {
                t.Stop();
                t.gameObject.SetActive(false);
            }
        }

    }

}