using System;
using Enemies.Leech;
using Systems;
using TMPro;
using UnityEngine;

namespace Pilot {

    public class ResourceItem : Pooling<ResourceItem> {

        private ItemType _type;
        private ItemType Type {
            get => _type;
            set {
                if (Application.isPlaying)
                    name = $"ResourceItem.{value.ToString()}";
                _type = value;
                if (!spinner)
                    spinner = transform.GetChild(0);
                for (int i = 0; i < itemMeshes.Length; i++)
                    itemMeshes[i].SetActive(i == (int)value);

                tag = value == ItemType.Debris ? "Debris" : "Item";
            }
        }
        private Rigidbody2D _rb;
        public Rigidbody2D Rb {
            get {
                if (_rb == null)
                    _rb = GetComponent<Rigidbody2D>();
                return _rb;
            }
        }
        
        public LeechCore chaser;
        [SerializeField] private GameObject[] itemMeshes;
        [SerializeField] private Transform spinner;
        [SerializeField] private float spinSpeed;
        [SerializeField] private float lifetime;
        [SerializeField] private float blinkStart;
        [SerializeField] private float blinkFrequency;

        private float spawnTime;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            if (!spinner)
                spinner = transform.GetChild(0);
            Type = Type; // Force the corresponding mesh to be made visible on start
        }

        // Update is called once per frame
        void Update() {
            spinner.transform.Rotate(Time.deltaTime * spinSpeed * Vector3.up, Space.Self);
            bool show = true;
            float blinkSin = Mathf.Sin((Time.time - spawnTime) * Mathf.PI * blinkFrequency);
            Debug.DrawRay(transform.position, Vector2.up * blinkSin, Color.dodgerBlue);
            if (Time.time - spawnTime > blinkStart)
                show = blinkSin > 0;
            itemMeshes[(int)Type].SetActive(show);
            if (Time.time - spawnTime >= lifetime)
                Type = ItemType.Debris;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player"))
                PickUp();
            if (other.CompareTag("KillScreen"))
                Stash();
        }

        public void LeechConsume(Vector2 pos) {
            Type = ItemType.Debris;
            transform.position = pos;
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
            spawnTime = Time.time;
            gameObject.SetActive(true);
            Type = (ItemType)p[0];
        }
        protected override void Disable() {
            gameObject.SetActive(false);
            chaser = null;
        }

    }

}