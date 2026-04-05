using UnityEngine;

namespace Systems {
    public abstract class Singleton<T> : MonoBehaviour where T : UnityEngine.Object {
        private static T _instance;
        public static T Instance {
            get {
                if (_instance == null)
                    _instance = FindAnyObjectByType<T>();
                return _instance;
            }
        }
    }
}