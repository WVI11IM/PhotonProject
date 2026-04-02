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

    public NetworkRunner networkRunnerPrefab;
    public NetworkObject playerNetworkPrefab;

    [SerializeField] private NetworkPrefabRef pilotPrefab;
    [SerializeField] private NetworkPrefabRef engineerPrefab;

    public Dictionary<PlayerRef, NetworkPlayerData> connectedPlayers = new Dictionary<PlayerRef, NetworkPlayerData>();
    //Tracks which player already has their gameplay object spawned
    private Dictionary<PlayerRef, NetworkObject> playerToSpawnedObject = new Dictionary<PlayerRef, NetworkObject>();

    NetworkRunner networkRunner;
    public NetworkRunner Runner => networkRunner;

    private SessionListUIHandler sessionListUI;
    private RoomPlayerListUIHandler roomUI;
    [HideInInspector] public bool justLeftRoom = false;
    private bool gameplayObjectsSpawned = false;


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
            var obj = networkRunner.Spawn(playerNetworkPrefab, Vector3.zero, Quaternion.identity, networkRunner.LocalPlayer);
            networkRunner.SetPlayerObject(networkRunner.LocalPlayer, obj);
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
        if (!networkRunner.IsSharedModeMasterClient)
            return;
        playerToSpawnedObject.Clear();
        gameplayObjectsSpawned = false;
        networkRunner.LoadScene(SceneRef.FromIndex(GAMEPLAY_SCENE_INDEX));
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
        //Re-register callbacks just in case
        if (networkRunner != null)
            networkRunner.AddCallbacks(this);

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
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        //If the active scene is the Room scene, finds all PlayerNetwork objects to update player info
        if (currentScene == ROOM_SCENE_INDEX)
        {
            if (runner.GetPlayerObject(runner.LocalPlayer) == null)
            {
                //Spawns the local player only after scene has already loaded
                var obj = runner.Spawn(playerNetworkPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
                runner.SetPlayerObject(runner.LocalPlayer, obj);
            }

            gameplayObjectsSpawned = false;


            PlayerNetwork[] others = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
            foreach (var p in others)
            {
                OnPlayerInfoUpdated(p);
            }
        }

        if (currentScene == GAMEPLAY_SCENE_INDEX)
        {
            NetworkObject myLobbyObj = runner.GetPlayerObject(runner.LocalPlayer);
            if (myLobbyObj != null)
            {
                myLobbyObj.GetComponent<PlayerNetwork>().IsReadyInGameplay = true;
                Debug.Log("[Handshake] Lobby Object is ready for Gameplay spawning!");
            }
        }
    }

    private void Update()
    {
        //If the active scene is the Gameplay scene but the objects haven't been spawned yet...
        if (SceneManager.GetActiveScene().buildIndex == GAMEPLAY_SCENE_INDEX && !gameplayObjectsSpawned)
        {
            if (networkRunner.IsSharedModeMasterClient)
            {
                SpawnGameplayObjects(networkRunner);
            }
        }
    }

    void SpawnGameplayObjects(NetworkRunner runner)
    {
        if (gameplayObjectsSpawned) return;
        if (!runner.IsSharedModeMasterClient) return;
        //Waits for all players to join in first
        if (runner.ActivePlayers.Count() < 2) return;

        foreach (PlayerRef playerRef in runner.ActivePlayers)
        {
            //SKIP if we already spawned an object for this specific player
            if (playerToSpawnedObject.ContainsKey(playerRef)) continue;

            NetworkObject lobbyGhost = runner.GetPlayerObject(playerRef);
            if (lobbyGhost == null) return;

            PlayerNetwork lobbyScript = lobbyGhost.GetComponent<PlayerNetwork>();
            if (!lobbyScript.IsReadyInGameplay || lobbyScript.playerRole == Role.None) return;

            NetworkPrefabRef selectedPrefab = (lobbyScript.playerRole == Role.Engineer) ? engineerPrefab : pilotPrefab;

            //Spawn the Gameplay Version
            NetworkObject spawnedObj = runner.Spawn(selectedPrefab, Vector3.zero, Quaternion.identity, playerRef, (runner, obj) =>
            {
                //Set authority and pass data from Lobby version to Gameplay version
                obj.AssignInputAuthority(playerRef);

                var newPlayerScript = obj.GetComponent<PlayerNetwork>();
                newPlayerScript.playerName = lobbyScript.playerName;
                newPlayerScript.playerRole = lobbyScript.playerRole;
            });

            playerToSpawnedObject[playerRef] = spawnedObj;

            //Sets the new pilot/engineer as the official playerobject for the PlayerRef
            runner.SetPlayerObject(playerRef, spawnedObj);

            //Despawns the "Ghost" that was only used for the Room
            runner.Despawn(lobbyGhost);

            Debug.Log($"Handover complete for {playerRef}: Ghost despawned, Body assigned.");
        }

        if (playerToSpawnedObject.Count == 2)
        {
            gameplayObjectsSpawned = true;
            Debug.Log("SUCCESS: All roles spawned and assigned.");

            //Find the objects in dictionary to link
            NetworkObject pilotObj = playerToSpawnedObject.Values.FirstOrDefault(x => x.GetComponent<PilotItemSender>() != null);
            NetworkObject engineerObj = playerToSpawnedObject.Values.FirstOrDefault(x => x.GetComponent<PilotItemReceiver>() != null);

            if (pilotObj != null && engineerObj != null)
            {
                pilotObj.GetComponent<PilotItemSender>().AssignEngineer(engineerObj);
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
