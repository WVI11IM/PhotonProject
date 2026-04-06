using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkRunnerHandler Instance;

    [Header("Prefabs & References")]
    public NetworkRunner networkRunnerPrefab;
    [SerializeField] private NetworkPrefabRef pilotPrefab;
    [SerializeField] private NetworkPrefabRef engineerPrefab;

    public Dictionary<PlayerRef, NetworkPlayerData> connectedPlayers = new Dictionary<PlayerRef, NetworkPlayerData>();

    NetworkRunner networkRunner;
    public NetworkRunner Runner => networkRunner;

    private SessionListUIHandler sessionListUI;
    private RoomPlayerListUIHandler roomUI;
    [HideInInspector] public bool justLeftRoom = false;
    private bool systemsLinked = false;


    //Index of lobby scene
    public const int LOBBY_SCENE_INDEX = 0;
    //Index of session room scene
    public const int ROOM_SCENE_INDEX = 1;
    //Index of engineer scene
    public const int GAMEPLAY_SCENE_INDEX = 2;

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
            return;
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
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    public void RegisterSessionListUI(SessionListUIHandler ui) => sessionListUI = ui;
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

    //Called after role and name assingment panel confirmation
    public async void StartBrowsingSessions()
    {
        //Wait till runner is ready
        await networkRunner.JoinSessionLobby(SessionLobby.Shared);

        sessionListUI.OnLookingForGameSessions();
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
    public void LeaveRoom()
    {
        connectedPlayers.Clear();
        justLeftRoom = true;
        if (networkRunner != null && networkRunner.IsRunning)
        {
            networkRunner.Shutdown();
        }

        if (SceneManager.GetActiveScene().buildIndex != LOBBY_SCENE_INDEX)
        {
            SceneManager.LoadScene(LOBBY_SCENE_INDEX);
        }
        systemsLinked = false;
    }

    public void KickPlayer(PlayerRef playerRef)
    {
        if (!networkRunner.IsSharedModeMasterClient)
            return;

        NetworkObject obj = Runner.GetPlayerObject(playerRef);

        if (obj == null)
            return;

        PlayerNetwork player = obj.GetComponent<PlayerNetwork>();

        Debug.Log($"SENDING KICK RPC TO: {player.playerName}");

        player.RPC_KickPlayer();
    }

    public async void StartGame()
    {
        if (!networkRunner.IsSharedModeMasterClient)
            return;

        PlayerNetwork[] allPlayers = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            p.RPC_SetGameplayActive();
        }
        await System.Threading.Tasks.Task.Delay(200);

        await networkRunner.LoadScene(SceneRef.FromIndex(GAMEPLAY_SCENE_INDEX));
    }

    //Coroutine that waits for both Pilot and Engineer to exist in scene
    //in order to link them together through RPCs
    private IEnumerator LinkSystemsRoutine()
    {
        PlayerRef pilotRef = default;
        PlayerRef engineerRef = default;

        foreach (var kvp in connectedPlayers)
        {
            if (kvp.Value.playerRole == Role.Pilot)
                pilotRef = kvp.Key;

            if (kvp.Value.playerRole == Role.Engineer)
                engineerRef = kvp.Key;
        }

        NetworkObject pilotObj = null;
        NetworkObject engineerObj = null;

        while (pilotObj == null || engineerObj == null)
        {
            pilotObj = Runner.GetPlayerObject(pilotRef);
            engineerObj = Runner.GetPlayerObject(engineerRef);

            yield return null;
        }

        var pilotSender = pilotObj.GetComponentInChildren<Pilot.PilotItemSender>(true);
        var engineerSender = engineerObj.GetComponentInChildren<EngineerItemSender>(true);

        if (pilotSender != null)
        {
            pilotSender.RPC_AssignEngineer(engineerObj);
        }

        if (engineerSender != null)
        {
            engineerSender.RPC_AssignPilot(pilotObj);
        }

        Debug.Log(">>Pilot and Engineer systems linked!!!<<");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Re-register callbacks just in case
        if (networkRunner != null)
        {
            networkRunner.RemoveCallbacks(this);
            networkRunner.AddCallbacks(this);
        }

        if (scene.buildIndex == LOBBY_SCENE_INDEX) StartCoroutine(EnableLobbyUI());

        OnSceneLoadDone(networkRunner);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (Instance != this) return;

        int currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == ROOM_SCENE_INDEX)
        {
            StartCoroutine(DelayedSpawnRoutine(runner));
        }
    }

    private IEnumerator DelayedSpawnRoutine(NetworkRunner runner)
    {
        yield return new WaitUntil(() => runner.IsRunning);

        if (runner.GetPlayerObject(runner.LocalPlayer) == null)
        {
            NetworkPrefabRef prefabToSpawn = PlayerInfo.Role == Role.Engineer ? engineerPrefab : pilotPrefab;
            var obj = runner.Spawn(prefabToSpawn, Vector3.zero, Quaternion.identity, runner.LocalPlayer);

            runner.SetPlayerObject(runner.LocalPlayer, obj);
        }
    }

    //Coroutine for when player leaves Room scene or gets kicked out of a session
    //They get redirected to the sessions list and don't need to write down name and roles again
    private IEnumerator EnableLobbyUI()
    {
        yield return new WaitForSeconds(0.2f);

        if (sessionListUI != null && justLeftRoom)
        {
            justLeftRoom = false;

            sessionListUI.ReturnFromSessionLeave();
            StartBrowsingSessions();
        }
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

        if (connectedPlayers.Count == 2 &&
            Runner.IsSharedModeMasterClient &&
            !systemsLinked)
        {
            systemsLinked = true;
            StartCoroutine(LinkSystemsRoutine());
        }
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (sessionListUI == null) return;
        sessionListUI.ClearList();

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

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (Runner.IsSharedModeMasterClient)
            TryLinkPlayerSystems();
    }
    public void TryLinkPlayerSystems()
    {
        if (!Runner.IsSharedModeMasterClient) return;

        NetworkObject pilotObj = null;
        NetworkObject engineerObj = null;

        PlayerRef pilotRef = connectedPlayers.FirstOrDefault(x => x.Value.playerRole == Role.Pilot).Key;
        PlayerRef engineerRef = connectedPlayers.FirstOrDefault(x => x.Value.playerRole == Role.Engineer).Key;

        if (!pilotRef.Equals(default))
        {
            pilotObj = Runner.GetPlayerObject(pilotRef);
        }
        if (!engineerRef.Equals(default))
        {
            engineerObj = Runner.GetPlayerObject(engineerRef);
        }

        if (pilotObj != null && engineerObj != null)
        {
            pilotObj.GetComponentInChildren<Pilot.PilotItemSender>(true)?.RPC_AssignEngineer(engineerObj);
            engineerObj.GetComponentInChildren<EngineerItemSender>(true)?.RPC_AssignPilot(pilotObj);
        }
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
