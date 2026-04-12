using UnityEngine;

namespace Misc {

    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class CameraFOVFixer : MonoBehaviour {

        private Camera _camera;
        [SerializeField] private float fov;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _camera = GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update() {
            _camera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(fov, _camera.aspect);
        }

    }

}