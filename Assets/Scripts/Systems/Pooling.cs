using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems {

    /// <summary>
    /// Abstract class for objects that should implement pooling (i.e. objects that are instantiated/destroyed frequently, e.g. bullets, items.)
    /// </summary>
    /// <typeparam name="T">The type of object that will be pooled (should be the same type as the object that is inheriting from this class.)</typeparam>
    /// <remarks>Working with templated inheritance is making my brain operate in non-euclidean space I stg...</remarks>
    public abstract class Pooling<T> : MonoBehaviour where T : Pooling<T> {

        private static Stack<T> _instances = new();
        private static T _prefab;
        private static T Prefab {
            get {
                string path = $"Prefabs/Pooling/{typeof(T)}";
                if (_prefab == null)
                    _prefab = Resources.Load<T>(path);
                if (_prefab == null)
                    Debug.LogError($"Unable to load prefab for Pooled object {typeof(T)}. Please ensure a prefab with a {typeof(T)} component is present at {path}.");
                return _prefab;
            }
        }

        /// <summary>
        /// Retrieves a pooled instance, or creates a new one if the stack is empty
        /// </summary>
        public static T Retrieve(params object[] p) {
            T instance = null;

            if (_instances.Count == 0) {
                instance = Instantiate(Prefab);
            } else
                instance = _instances.Pop();
            instance.Initialize(p);
            instance.hideFlags = HideFlags.None;
            return instance;
        }

        public void Stash() {
            hideFlags = HideFlags.HideInHierarchy;
            _instances.Push((T)this);
            Disable();
        }

        protected abstract void Initialize(params object[] p);
        protected abstract void Disable();

    }

}