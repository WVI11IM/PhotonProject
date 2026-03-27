using UnityEngine;
using TMPro;
using Fusion;
using UnityEngine.UI;
using System;

public class SessionListUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject sessionItemListPrefab;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;

    [SerializeField] private GameObject createSessionPanel;
    [SerializeField] private GameObject sessionListPanel;
    [SerializeField] private Button createNewSessionButton;

    private void Start()
    {
        if (createNewSessionButton != null)
            createNewSessionButton.onClick.AddListener(OpenCreateSessionPanel);

        NetworkRunnerHandler.Instance.RegisterSessionListUI(this);
    }

    public void ClearList()
    {
        //Delete all children of layout group
        foreach (Transform child in verticalLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        //Hide the status message
        statusText.gameObject.SetActive(false);
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        //Add a new item to the list
        SessionInfoListUIItem addedItem = Instantiate(sessionItemListPrefab, verticalLayoutGroup.transform).GetComponent<SessionInfoListUIItem>();

        addedItem.SetInformation(sessionInfo);

        addedItem.OnJoinSession += AddedSessionInfoUIItem_OnJoinSession;
    }

    private void AddedSessionInfoUIItem_OnJoinSession(SessionInfo info)
    {
        NetworkRunnerHandler.Instance.JoinSession(info);
    }

    public void OnNoSessionsFound()
    {
        statusText.text = "No game session found";
        statusText.gameObject.SetActive(true);
    }

    public void OnLookingForGameSessions()
    {
        statusText.text = "Looking for game sessions";
        statusText.gameObject.SetActive(true);
    }

    private void OpenCreateSessionPanel()
    {
        if (createSessionPanel != null)
        {
            sessionListPanel.SetActive(false);
            createSessionPanel.SetActive(true);
        }
    }

    public void CloseCreateSessionPanel()
    {
        if (createSessionPanel != null)
        {
            createSessionPanel.SetActive(false);
            sessionListPanel.SetActive(true);
        }
    }
}
