using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : Singleton<NetworkRunnerHandler>, INetworkRunnerCallbacks
{
    public NetworkRunner networkRunnerPrefab;

    public Dictionary<PlayerRef, NetworkPlayerData> connectedPlayers = new Dictionary<PlayerRef, NetworkPlayerData>();

    NetworkRunner networkRunner;
    public NetworkRunner Runner => networkRunner;

    private SessionListUIHandler sessionListUI;
    private RoomPlayerListUIHandler roomUI;

    //Index of lobby scene
    public const int LOBBY_SCENE_INDEX = 0;
    //Index of session room scene
    public const int ROOM_SCENE_INDEX = 1;
    //Index of engineer scene
    public const int ENGINEER_SCENE_INDEX = 2;
    //Index of pilot scene
    public const int PILOT_SCENE_INDEX = 3;

    protected override void Awake()
    {
        base.Awake();

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
            GameMode = GameMode.Client,
            SessionName = sessionInfo.Name,
            Scene = sceneInfo,
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
        });

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
            return;
        }

        //Join the shared lobby (NOT A SESSION, not yet)
        await networkRunner.JoinSessionLobby(SessionLobby.Shared);

        //Updates the UI
        sessionListUI.OnLookingForGameSessions();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!connectedPlayers.ContainsKey(player))
        {
            if (player == runner.LocalPlayer)
            {
                connectedPlayers[player] =
                    new NetworkPlayerData(PlayerInfo.Name, PlayerInfo.Role, player);
            }
            else
            {
                //STILL NEED TO PROPERLY SEND RPC FOR OTHER PLAYER'S INFO TO BE FILLED IN
                connectedPlayers[player] =
                    new NetworkPlayerData("Player", Role.None, player);
            }
        }

        if (roomUI != null)
        {
            roomUI.UpdatePlayerList(connectedPlayers);
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

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

    public void KickPlayer(string playerName)
    {
        if (!networkRunner.IsServer)
            return;

        foreach (var kvp in connectedPlayers)
        {
            if (kvp.Value.playerName == playerName)
            {
                networkRunner.Disconnect(kvp.Key);
                break;
            }
        }
    }

    public void LeaveRoom()
    {
        //Debug.Log("LEAVE ROOM!!");
        if (networkRunner == null)
        {
            SceneManager.LoadScene(LOBBY_SCENE_INDEX);
            return;
        }

        if (networkRunner.IsServer)
        {
            //Disconnects all clients before shutting down
            foreach (var player in connectedPlayers.Keys)
            {
                if (player != networkRunner.LocalPlayer)
                    networkRunner.Disconnect(player);
            }
        }

        networkRunner.Disconnect(networkRunner.LocalPlayer);
        connectedPlayers.Clear();
        SceneManager.LoadScene(LOBBY_SCENE_INDEX);
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
