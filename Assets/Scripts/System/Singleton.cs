using UnityEngine;

namespace System {
    public abstract class Singleton<T> : MonoBehaviour where T : UnityEngine.Object {
        private T _instance;
        public T Instance {
            get {
                if (_instance == null)
                    _instance = FindAnyObjectByType<T>();
                return _instance;
            }
        }
    }
}