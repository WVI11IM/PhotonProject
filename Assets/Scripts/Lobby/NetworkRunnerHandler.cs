using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner networkRunnerPrefab;

    NetworkRunner networkRunner;
    [SerializeField] private SessionListUIHandler sessionListUI;

    private void Awake()
    {
        NetworkRunner networkRunnerInScene = FindFirstObjectByType<NetworkRunner>();

        //if we already have a network runner on scene, we shouldn't create another one, use the existing one
        if (networkRunnerInScene != null)
            networkRunner = networkRunnerInScene;
    }

    private async void Start()
    {
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network Runner";
            //Register this script to receive Fusion events
            networkRunner.AddCallbacks(this);
        }
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        sessionListUI.ClearList();

        //If there are no sessions...
        if (sessionList.Count == 0)
        {
            sessionListUI.OnNoSessionsFound();
            return;
        }

        foreach (SessionInfo session in sessionList)
        {
            sessionListUI.AddToList(session);
        }
    }

    //Creates a session with given name

    //THIS ONLY CREATES THE SESSION IN FUSION "ON PAPER"
    //WE STILL NEED TO MAKE PROPER SCENE TRANSITIONS AND ROLE-SPECIFIC SCENES, those are not yet implemented
    public async void CreateSession(string sessionName)
    {
        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            Scene = sceneInfo,
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 2
        });
    }

    //Join an existing session
    public async void JoinSession(SessionInfo sessionInfo)
    {
        await networkRunner.Shutdown();

        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionInfo.Name,
            Scene = sceneInfo,
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
        });
    }

    //Called after role and name assingment
    public async void StartBrowsingSessions()
    {
        if (networkRunner == null)
        {
            return;
        }

        //Join the shared lobby (NOT A SESSION, not yet)
        await networkRunner.JoinSessionLobby(SessionLobby.Shared);

        //Updates the UI
        sessionListUI.OnLookingForGameSessions();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
