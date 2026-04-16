using UnityEngine;

public class RoleCamera : MonoBehaviour
{
    [SerializeField] private bool isActiveCamera;

    private Camera _cam;
    private AudioListener _listener;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _listener = GetComponent<AudioListener>();
    }

    void LateUpdate()
    {
        var player = PlayerNetwork.Local;

        if (player == null)
            return;

        bool isPilot = player.playerRole == Role.Pilot;
        bool isEngineer = player.playerRole == Role.Engineer;

        //Enable the camera for local player only
        bool isMine = player.Object.HasInputAuthority;

        _cam.enabled = isMine;
        if (_listener) _listener.enabled = isMine;
    }
}