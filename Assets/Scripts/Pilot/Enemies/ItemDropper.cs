using System;
using Systems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pilot.Enemies {

    [Serializable]
    public struct ItemDropConfig {
        public ItemType type;
        public int count;
    }

    public class ItemDropper : MonoBehaviour {

        public ItemDropConfig[] drops;
        public float explosionForce;

        public void DropItems() {
            foreach (ItemDropConfig drop in drops)
                for (int i = 0; i < drop.count; i++) {
                    ResourceItem instance = Pooling<ResourceItem>.Retrieve(drop.type);
                    instance.transform.position = transform.position;
                    instance.Rb.AddForce(Random.insideUnitCircle * explosionForce);
                }
        }

    }

}