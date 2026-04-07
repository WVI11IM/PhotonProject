using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerNetwork : NetworkBehaviour
{
    [Networked] public string playerName { get; set; }
    [Networked] public Role playerRole { get; set; }

    [Networked, OnChangedRender(nameof(OnGameplayActiveChanged))]
    public NetworkBool IsGameplayActive { get; set; }

    [Header("Visibility References")]
    public GameObject[] gameplayVisuals;
    public MonoBehaviour[] gameplayScripts;

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        base.Spawned();

        //When player is spawned, it attempts to link their role script to the other player's
        if (Object.HasStateAuthority)
        {
            NetworkRunnerHandler.Instance.TryLinkPlayerSystems();
        }

        //Ensures this object persists from Room to Gameplay scene
        Runner.MakeDontDestroyOnLoad(gameObject);

        //Initialize built-in change detector to watch this object's networked state
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasInputAuthority)
        {
            playerName = PlayerInfo.Name;
            playerRole = PlayerInfo.Role;
        }

        UpdateState();
        UpdateLocalUI();

        if (!Object.HasInputAuthority)
        {
            StartCoroutine(InitialSyncDelay());
        }
    }

    IEnumerator InitialSyncDelay()
    {
        yield return null;
        yield return null;
        yield return null;

        while (string.IsNullOrEmpty(playerName))
        {
            yield return new WaitForSeconds(0.2f);
        }
        UpdateLocalUI();
    }
    public override void Render()
    {
        if (_changeDetector != null)
        {
            foreach (var change in _changeDetector.DetectChanges(this, true))
            {
                if (change == nameof(playerName) || change == nameof(playerRole))
                {
                    UpdateLocalUI();
                }
            }
        }
    }
    public void OnGameplayActiveChanged()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        //Toggle visuals
        foreach (var visual in gameplayVisuals)
        {
            if (visual != null) visual.SetActive(IsGameplayActive);
        }

        //Toggle scripts
        foreach (var script in gameplayScripts)
        {
            if (script != null) script.enabled = IsGameplayActive;
        }
    }

    private void UpdateLocalUI()
    {
        if (NetworkRunnerHandler.Instance != null)
        {
            //Handler reads this script's playerName and playerRole for local UI update
            NetworkRunnerHandler.Instance.OnPlayerInfoUpdated(this);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.InputAuthority)]
    public void RPC_KickPlayer()
    {
        NetworkRunnerHandler.Instance.LeaveRoom();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetGameplayActive()
    {
        IsGameplayActive = true;
    }
}
