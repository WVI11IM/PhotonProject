using System;
using Systems;
using TMPro;
using UnityEngine;

namespace Pilot {

    public class ResourceItem : Pooling<ResourceItem> {

        [SerializeField] private ItemType type;
        [SerializeField] private GameObject cube;
        [SerializeField] private TextMeshPro label;
        private Rigidbody2D _rb;
        public Rigidbody2D Rb {
            get {
                if (_rb == null)
                    _rb = GetComponent<Rigidbody2D>();
                return _rb;
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            label.text = type.ToString();
        }

        // Update is called once per frame
        void Update() {
            cube.transform.Rotate(Vector3.up * Time.deltaTime, Space.Self);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player"))
                PickUp();
        }

        public void LeechConsume() {
            Destroy(gameObject);
        }

        private void PickUp() {
            try {
                PilotItemSender.Instance.SendItem(type);
            }
            catch {
                // ignored
            }
            Stash();
        }

        protected override void Initialize(params object[] p) {
            gameObject.SetActive(true);
            type = (ItemType)p[0];
            label.text = type.ToString();
        }
        protected override void Disable() {
            gameObject.SetActive(false);
        }

    }

}