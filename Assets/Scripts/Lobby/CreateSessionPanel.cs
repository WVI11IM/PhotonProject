using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateSessionPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField sessionNameInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private GameObject sessionListPanel;
    [SerializeField] private GameObject createSessionPanel;

    //This script checks for the session's name and assigns the buttons' functions for panel transitions

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);

        UpdateConfirmButtonState();

        sessionNameInput.onValueChanged.AddListener(_ => UpdateConfirmButtonState());
    }

    private void UpdateConfirmButtonState()
    {
        confirmButton.interactable = !string.IsNullOrEmpty(sessionNameInput.text);
    }

    private void OnConfirm()
    {
        string sessionName = sessionNameInput.text;

        if (string.IsNullOrEmpty(sessionName))
            return;

        NetworkRunnerHandler.Instance.CreateSession(sessionName);

        createSessionPanel.SetActive(false);
        sessionListPanel.SetActive(false);
    }

    private void OnCancel()
    {
        createSessionPanel.SetActive(false);
        sessionListPanel.SetActive(true);
    }
}