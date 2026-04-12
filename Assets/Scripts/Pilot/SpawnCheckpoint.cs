using System;
using System.Collections.Generic;
using Enemies.Leech;
using Enemies.Strider;
using JetBrains.Annotations;
using Pilot.Enemies;
using Systems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pilot {

    [ExecuteInEditMode]
    public class SpawnCheckpoint : MonoBehaviour {

        [SerializeField] private int strider;
        [SerializeField] private int leech;
        [SerializeField] private ItemDropConfig[] items;
        
        private Transform _target;
        private readonly List<Transform> _allChildren = new();
        private List<Transform> _eligibleChildren = new();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            if (Camera.main != null) _target = Camera.main.transform;
        }

        // Update is called once per frame
        void Update() {
            for (int i = 0; i < transform.childCount; i++)
                Debug.DrawLine(transform.position, transform.GetChild(i).position, Color.brown);
            if (!Application.isPlaying)
                return;
            if (_target.position.y > transform.position.y) {
                for (var i = 0; i < strider; i++) {
                    StriderCore inst = Pooling<StriderCore>.Retrieve();
                    inst.transform.position = GetRandomChild().position;
                }
                for (var i = 0; i < leech; i++) {
                    LeechCore inst = Pooling<LeechCore>.Retrieve();
                    inst.transform.position = GetRandomChild().position;
                }
                for (var i = 0; i < items.Length; i++) {
                    for (var j = 0; j < items[i].count; j++) {
                        ResourceItem inst = Pooling<ResourceItem>.Retrieve(items[i].type);
                        inst.transform.position = GetRandomChild().position;
                    }
                }
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Returns a random child without picking the same one twice (unless all children have been picked)
        /// </summary>
        /// <returns>A unique child</returns>
        private Transform GetRandomChild() {
            // Store all children in a list
            if (_allChildren.Count == 0)
                for (int i = 0; i < transform.childCount; i++)
                    _allChildren.Add(transform.GetChild(i));
            // If there are no eligible children, reset the eligible children list to all children
            if (_eligibleChildren.Count == 0)
                _eligibleChildren = new List<Transform>(_allChildren);
            // Pick a random eligible child, then remove it from the list of eligible children
            Transform child = _eligibleChildren[Random.Range(0, _eligibleChildren.Count)];
            _eligibleChildren.Remove(child);
            return child;
        }

    }

}