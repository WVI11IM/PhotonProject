using System;
using TMPro;
using UnityEngine;

namespace Pilot {

    public class ResourceItem : MonoBehaviour {

        [SerializeField] private ItemType type;
        [SerializeField] private GameObject cube;
        [SerializeField] private TextMeshPro label;

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
            Destroy(gameObject);
        }

    }

}