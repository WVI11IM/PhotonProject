using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerListUIItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerRoleText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Button kickButton;

    public string playerName;
    public Role playerRole;

    public event Action<string> OnKickPlayer;

    //Sets information for player's name and role and if the kick button should be shown

    public void SetInformation(string name, Role role, bool showKickButton)
    {
        playerName = name;
        playerRole = role;

        playerNameText.text = name;
        playerRoleText.text = role.ToString();

        kickButton.gameObject.SetActive(showKickButton);
    }

    public void OnKickClicked()
    {
        OnKickPlayer?.Invoke(playerName);
    }
}
