using Pilot;
using Systems;
using UnityEditor;
using UnityEngine;

namespace Misc {

    public class GameOver : Singleton<GameOver> {

        private static readonly int animTriggerShow = Animator.StringToHash("Show");

        [SerializeField] private Transform[] points;

        private Animator anim;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update() { }

        public void VineBoom() {
            Juice.Instance.AddShake(1f);
            Juice.Instance.InvokeHitFreeze();
        }

        public void Particles(int which) {
            Particles p = Pooling<Particles>.Retrieve(BulletType.Player, 2);
            p.transform.parent = points[which];
            p.transform.localPosition = Vector3.zero;
            p.transform.localRotation = Quaternion.identity;
        }

        [MenuItem("Debug/Show Game Over")]
        public static void Show() {
            Instance.anim.SetTrigger(animTriggerShow);
        }

    }

}