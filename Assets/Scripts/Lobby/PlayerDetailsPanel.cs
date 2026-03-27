using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerDetailsPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button pilotButton;
    [SerializeField] private Button engineerButton;
    [SerializeField] private Button confirmButton;

    [SerializeField] private GameObject sessionListPanel;
    [SerializeField] private GameObject detailsPanel;

    private Role selectedRole = Role.None;

    //This script checks for the player's name and role to update buttons and panels.
    //It also assigns the buttons' functions for panel transitions

    private void Start()
    {
        pilotButton.onClick.AddListener(() => SelectRole(Role.Pilot));
        engineerButton.onClick.AddListener(() => SelectRole(Role.Engineer));

        confirmButton.onClick.AddListener(ConfirmPlayerInfo);

        nameInput.onValueChanged.AddListener(_ => UpdateConfirmButtonState());

        UpdateConfirmButtonState();
        ResetRoleButtonColors();
    }

    private void SelectRole(Role role)
    {
        selectedRole = role;
        UpdateRoleButtonColors();
        UpdateConfirmButtonState();
        Debug.Log("Selected role: " + role);
    }
    private void UpdateRoleButtonColors()
    {
        ResetRoleButtonColors();

        if (selectedRole == Role.Pilot)
            pilotButton.image.color = Color.green;
        else if (selectedRole == Role.Engineer)
            engineerButton.image.color = Color.green;
    }
    private void ResetRoleButtonColors()
    {
        pilotButton.image.color = Color.white;
        engineerButton.image.color = Color.white;
    }

    private void UpdateConfirmButtonState()
    {
        confirmButton.interactable = !string.IsNullOrEmpty(nameInput.text) && selectedRole != Role.None;
    }

    //Only allows player to go to lobby if they have chosen a role and given a name
    private void ConfirmPlayerInfo()
    {
        string playerName = nameInput.text;

        if (string.IsNullOrEmpty(playerName) || selectedRole == Role.None)
        {
            Debug.LogWarning("Enter name and select a role!");
            return;
        }

        PlayerInfo.Name = playerName;
        PlayerInfo.Role = selectedRole;

        detailsPanel.SetActive(false);
        sessionListPanel.SetActive(true);

        NetworkRunnerHandler.Instance.StartBrowsingSessions();
    }
}