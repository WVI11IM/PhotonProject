using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;

public class SessionInfoListUIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button joinButton;

    SessionInfo sessionInfo;

    public event Action<SessionInfo> OnJoinSession;

    //Sets information for session name and number of players
    public void SetInformation(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;

        sessionNameText.text = sessionInfo.Name;
        playerCountText.text = $"{sessionInfo.PlayerCount.ToString()}/{sessionInfo.MaxPlayers.ToString()}";

        bool isJoinButtonActive = true;

        if(sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
            isJoinButtonActive = false;

        joinButton.gameObject.SetActive(isJoinButtonActive);

    }

    public void OnClick()
    {
        OnJoinSession?.Invoke(sessionInfo);
    }
}
