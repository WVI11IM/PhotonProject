using System;
using Systems;
using TMPro;
using UnityEngine;

namespace Pilot {

    public class ResourceItem : Pooling<ResourceItem> {

        private ItemType _type;
        [field:SerializeField] private ItemType Type {
            get => _type;
            set {
                if (Application.isPlaying)
                    name = $"ResourceItem.{value.ToString()}";
                _type = value;
                if (!spinner)
                    spinner = transform.GetChild(0);
                for (int i = 0; i < itemMeshes.Length; i++)
                    itemMeshes[i].SetActive(i == (int)value);
            }
        }

        [SerializeField] private GameObject[] itemMeshes;
        [SerializeField] private Transform spinner;
        [SerializeField] private float spinSpeed;
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
            if (!spinner)
                spinner = transform.GetChild(0);
        }

        // Update is called once per frame
        void Update() {
            spinner.transform.Rotate(Time.deltaTime * spinSpeed * Vector3.up, Space.Self);
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
                PilotItemSender.Instance.SendItem(Type);
            }
            catch {
                // ignored
            }
            Stash();
        }

        protected override void Initialize(params object[] p) {
            gameObject.SetActive(true);
            Type = (ItemType)p[0];
        }
        protected override void Disable() {
            gameObject.SetActive(false);
        }

    }

}