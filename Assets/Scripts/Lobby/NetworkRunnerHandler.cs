using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : NetworkBehaviour, INetworkRunnerCallbacks
{
    public static NetworkRunnerHandler Instance;

    public NetworkRunner networkRunnerPrefab;
    public NetworkObject playerNetworkPrefab;

    public Dictionary<PlayerRef, NetworkPlayerData> connectedPlayers = new Dictionary<PlayerRef, NetworkPlayerData>();

    NetworkRunner networkRunner;
    public NetworkRunner Runner => networkRunner;

    private SessionListUIHandler sessionListUI;
    private RoomPlayerListUIHandler roomUI;
    [HideInInspector] public bool justLeftRoom = false;


    //Index of lobby scene
    public const int LOBBY_SCENE_INDEX = 0;
    //Index of session room scene
    public const int ROOM_SCENE_INDEX = 1;
    //Index of engineer scene
    public const int ENGINEER_SCENE_INDEX = 2;
    //Index of pilot scene
    public const int PILOT_SCENE_INDEX = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        networkRunner = FindFirstObjectByType<NetworkRunner>();
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network Runner";
            DontDestroyOnLoad(networkRunner.gameObject);
        }
        //Register this script to receive Fusion events
        networkRunner.AddCallbacks(this);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void RegisterSessionListUI(SessionListUIHandler ui)
    {
        sessionListUI = ui;
    }
    public void RegisterRoomUI(RoomPlayerListUIHandler ui)
    {
        roomUI = ui;
        PlayerNetwork[] foundPlayers = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
        foreach(var p in foundPlayers)
        {
            OnPlayerInfoUpdated(p);
        }

        roomUI.UpdatePlayerList(connectedPlayers);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (sessionListUI != null)
        {
            sessionListUI.ClearList();

            //If there are no sessions...
            if (sessionList.Count == 0)
            {
                sessionListUI.OnNoSessionsFound();
                return;
            }

            foreach (var session in sessionList)
            {
                sessionListUI.AddToList(session);
            }
        }
    }

    //Creates a session with given name and redirects player to Room scene
    public async void CreateSession(string sessionName)
    {
        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(ROOM_SCENE_INDEX));

        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            Scene = sceneInfo,
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 2
        });

        //Spawns networked player prefab for this client
        if (playerNetworkPrefab != null)
        {
            networkRunner.Spawn(playerNetworkPrefab, Vector3.zero, Quaternion.identity, networkRunner.LocalPlayer);
        }

        if (roomUI != null)
        {
            roomUI.SetRoomName(sessionName);
        }
    }

    //Join an existing session and redirects player to Room scene
    public async void JoinSession(SessionInfo sessionInfo)
    {
        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(ROOM_SCENE_INDEX));

        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = sessionInfo.Name,
            Scene = sceneInfo,
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (roomUI != null)
        {
            roomUI.SetRoomName(sessionInfo.Name);
        }
    }

    //Called after role and name assingment panel confirmation
    public async void StartBrowsingSessions()
    {
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network Runner";
            DontDestroyOnLoad(networkRunner.gameObject);
            networkRunner.AddCallbacks(this);
        }

        //Wait till runner is ready
        await networkRunner.JoinSessionLobby(SessionLobby.Shared);

        sessionListUI.OnLookingForGameSessions();
    }
    public void OnPlayerInfoUpdated(PlayerNetwork playerNetwork)
    {
        PlayerRef playerRef = playerNetwork.Object.InputAuthority;

        //Update local dictionary with latest synced data by overwriting placeholder items
        connectedPlayers[playerRef] = new NetworkPlayerData(
            playerNetwork.playerName,
            playerNetwork.playerRole,
            playerRef
        );

        if (roomUI != null)
        {
            roomUI.UpdatePlayerList(connectedPlayers);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (connectedPlayers.ContainsKey(player))
        {
            connectedPlayers.Remove(player);
        }

        if (roomUI != null)
        {
            roomUI.UpdatePlayerList(connectedPlayers);
        }
    }

    public void StartGame()
    {
        if (!networkRunner.IsServer)
            return;

        foreach (var player in connectedPlayers.Values)
        {
            //
            //WHEN STARTING THE GAME, EACH ROLE WILL HAVE THEIR DESIGNATED GAMEPLAY SCENE
            //
            if (player.playerRole == Role.Engineer)
            {
                //networkRunner.SetPlayerScene(player.playerRef, SceneRef.FromIndex(ENGINEER_SCENE_INDEX));
            }
            else if (player.playerRole == Role.Pilot)
            {
                //networkRunner.SetPlayerScene(player.playerRef, SceneRef.FromIndex(PILOT_SCENE_INDEX));
            }
        }
    }

    public void KickPlayer(PlayerRef playerRef)
    {
        if (!networkRunner.IsSharedModeMasterClient)
            return;

        PlayerNetwork[] players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p.Object.InputAuthority == playerRef)
            {
                Debug.Log($"SENDING KICK RPC TO: {p.playerName}");
                p.RPC_KickPlayer();
                return;
            }
        }
    }

    public async void LeaveRoom()
    {
        connectedPlayers.Clear();
        justLeftRoom = true;

        if (networkRunner != null && networkRunner.IsRunning)
        {
            await networkRunner.Shutdown();
        }

        if (SceneManager.GetActiveScene().buildIndex == LOBBY_SCENE_INDEX)
        {
            StopAllCoroutines();
            StartCoroutine(EnableLobbyUI());
        }
        else
        {
            SceneManager.LoadScene(LOBBY_SCENE_INDEX);
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == LOBBY_SCENE_INDEX)
        {
            StartCoroutine(EnableLobbyUI());
        }
    }

    //Using this coroutine for when player leaves Room scene or gets kicked out of a session
    //They get redirected to the sessions list and don't need to write down name and roles again
    private IEnumerator EnableLobbyUI()
    {
        yield return new WaitForSeconds(0.2f);

        if (sessionListUI != null)
        {
            if (justLeftRoom)
            {
                justLeftRoom = false;

                sessionListUI.ReturnFromSessionLeave();
                StartBrowsingSessions();
            }
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        //If the active scene is the Room scene, finds all PlayerNetwork objects to update player info
        if (SceneManager.GetActiveScene().buildIndex == ROOM_SCENE_INDEX)
        {
            if (runner.GetPlayerObject(runner.LocalPlayer) == null)
            {
                //Spawns the local player only after scene has already loaded
                runner.Spawn(playerNetworkPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
            }

            PlayerNetwork[] others = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
            foreach (var p in others)
            {
                OnPlayerInfoUpdated(p);
            }
        }
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
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

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
