using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkedUI : NetworkBehaviour
{
    [SerializeField] private GameObject[] uiElements;
    private bool uiVisible = false;

    public override void Spawned()
    {
        SetUIState(false);
    }

    public override void Render()
    {
        if (SceneManager.GetActiveScene().buildIndex != NetworkRunnerHandler.GAMEPLAY_SCENE_INDEX) return;

        //Only turn on the UI if it's the owner and it's not already visible
        if (Object != null && Object.HasInputAuthority && !uiVisible)
        {
            uiVisible = true;
            SetUIState(true);
            Debug.Log($"[UI] Authority confirmed for {gameObject.name}. Enabling UI.");
        }
    }

    public void SetUIState(bool state)
    {
        foreach (var element in uiElements)
        {
            if (element != null) element.SetActive(state);
        }
    }
}