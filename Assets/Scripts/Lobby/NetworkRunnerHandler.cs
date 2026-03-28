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

        //Spawns networked player prefab for this client
        if (playerNetworkPrefab != null)
        {
            networkRunner.Spawn(playerNetworkPrefab, Vector3.zero, Quaternion.identity, networkRunner.LocalPlayer);
        }

        if (roomUI != null)
        {
            roomUI.SetRoomName(sessionInfo.Name);
        }
    }

    //Called after role and name assingment
    public async void StartBrowsingSessions()
    {
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network Runner";
            DontDestroyOnLoad(networkRunner.gameObject);
            networkRunner.AddCallbacks(this);
        }

        await networkRunner.JoinSessionLobby(SessionLobby.Shared);

        sessionListUI.OnLookingForGameSessions();
    }
    public void OnPlayerInfoUpdated(PlayerNetwork playerNetwork)
    {
        PlayerRef playerRef = playerNetwork.Object.InputAuthority;

        if (!connectedPlayers.ContainsKey(playerRef))
        {
            //Add new entry
            connectedPlayers[playerRef] = new NetworkPlayerData(
                playerNetwork.playerName,
                playerNetwork.playerRole,
                playerRef
            );
        }
        else
        {
            //Update existing one
            connectedPlayers[playerRef].playerName = playerNetwork.playerName;
            connectedPlayers[playerRef].playerRole = playerNetwork.playerRole;
        }
        if (roomUI != null)
        {
            roomUI.UpdatePlayerList(connectedPlayers);
        }
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!connectedPlayers.ContainsKey(player))
        {
            if (player == runner.LocalPlayer)
            {
                //For the local player, use name/role immediately
                connectedPlayers[player] = new NetworkPlayerData(PlayerInfo.Name, PlayerInfo.Role, player);
            }
            else
            {
                //For others, use placeholder until their RPC updates (supposedly, because this is simply just not updating at all wtfff)
                connectedPlayers[player] = new NetworkPlayerData("Loading...", Role.None, player);
            }
        }

        if (roomUI != null)
        {
            roomUI.UpdatePlayerList(connectedPlayers);
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        connectedPlayers.Remove(player);

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
            //WHEN STARTING THE GAME, EACH ROLE WILL HAVE THEIR DESIGNATED GAMEPLAY SCENE
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

        networkRunner.Disconnect(playerRef);
    }

    public async void LeaveRoom()
    {
        connectedPlayers.Clear();
        justLeftRoom = true;

        if (networkRunner != null && networkRunner.IsRunning)
        {
            await networkRunner.Shutdown();
        }

        SceneManager.LoadScene(LOBBY_SCENE_INDEX);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == LOBBY_SCENE_INDEX)
        {
            StartCoroutine(EnableLobbyUI());
        }
    }
    private IEnumerator EnableLobbyUI()
    {
        //Wait one frame
        yield return null;

        if (sessionListUI != null)
        {
            if (justLeftRoom)
            {
                sessionListUI.ReturnFromSessionLeave();
                StartBrowsingSessions();
                justLeftRoom = false;
            }
        }
    }
    public void SetLocalPlayerInfo(string name, Role role)
    {
        if (networkRunner == null || !networkRunner.IsRunning)
            return;

        PlayerRef local = networkRunner.LocalPlayer;

        // Update own entry
        connectedPlayers[local] = new NetworkPlayerData(name, role, local);

        // Broadcast to all clients
        UpdateAllClientsPlayerList();
    }

    private void UpdateAllClientsPlayerList()
    {
        // Convert dictionary to arrays for RPC
        int count = connectedPlayers.Count;
        var names = new string[count];
        var roles = new Role[count];
        var refs = new PlayerRef[count];

        int i = 0;
        foreach (var kvp in connectedPlayers)
        {
            names[i] = kvp.Value.playerName;
            roles[i] = kvp.Value.playerRole;
            refs[i] = kvp.Key;
            i++;
        }

        RPC_UpdatePlayerList(names, roles, refs);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_UpdatePlayerList(string[] names, Role[] roles, PlayerRef[] refs)
    {
        connectedPlayers.Clear();

        for (int i = 0; i < names.Length; i++)
        {
            connectedPlayers[refs[i]] = new NetworkPlayerData(names[i], roles[i], refs[i]);
        }
        roomUI?.UpdatePlayerList(connectedPlayers);
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

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
