using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerListUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;

    [SerializeField] private GameObject playerItemPrefab;
    [SerializeField] private HorizontalLayoutGroup playerListLayout;

    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startGameButton;

    private const int MAX_PLAYERS = 2;
    private void Awake()
    {
        NetworkRunnerHandler.Instance.RegisterRoomUI(this);
    }
    private void Start()
    {
        //Updates the player list and room name

        UpdatePlayerList(NetworkRunnerHandler.Instance.connectedPlayers);
        if (NetworkRunnerHandler.Instance.Runner != null)
        {
            SetRoomName(NetworkRunnerHandler.Instance.Runner.SessionInfo.Name);
        }
    }

    public void SetRoomName(string roomName)
    {
        roomNameText.text = roomName;
    }

    public void ClearList()
    {
        foreach (Transform child in playerListLayout.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdatePlayerList(Dictionary<PlayerRef, NetworkPlayerData> players)
    {
        foreach (Transform child in playerListLayout.transform)
            Destroy(child.gameObject);

        bool hasPilot = false;
        bool hasEngineer = false;

        foreach (var kvp in players)
        {
            var data = kvp.Value;

            if (data.playerRole == Role.Pilot)
                hasPilot = true;
            if (data.playerRole == Role.Engineer)
                hasEngineer = true;

            var item = Instantiate(playerItemPrefab, playerListLayout.transform).GetComponent<RoomPlayerListUIItem>();
            item.OnKickPlayer += OnKickPlayer;

            bool isHost = NetworkRunnerHandler.Instance.Runner.IsServer;

            item.SetInformation(data.playerName, data.playerRole, isHost);
        }

        //The game can only start if room has a pilot and an engineer
        startGameButton.interactable = hasPilot && hasEngineer;

    }

    public void AddPlayer(string playerName, Role role, bool showKickButton)
    {
        if (playerListLayout.transform.childCount >= MAX_PLAYERS)
            return;
        RoomPlayerListUIItem item = Instantiate(playerItemPrefab, playerListLayout.transform).GetComponent<RoomPlayerListUIItem>();

        item.SetInformation(playerName, role, showKickButton);

        item.OnKickPlayer += OnKickPlayer;
    }

    private void OnKickPlayer(string playerName)
    {
        NetworkRunnerHandler.Instance.KickPlayer(playerName);
    }

    public void OnLeaveClicked()
    {
        NetworkRunnerHandler.Instance.LeaveRoom();
    }

    public void OnStartGameClicked()
    {
        NetworkRunnerHandler.Instance.StartGame();
    }
}