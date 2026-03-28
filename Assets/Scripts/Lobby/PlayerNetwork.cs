using Fusion;
using System.Collections;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Networked] public string playerName { get; set; }
    [Networked] public Role playerRole { get; set; }

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        //Initialize built-in change detector to watch this object's networked state
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasInputAuthority)
        {
            playerName = PlayerInfo.Name;
            playerRole = PlayerInfo.Role;

            UpdateLocalUI();
        }
        else
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
        if (_changeDetector == null) return;

        foreach (var change in _changeDetector.DetectChanges(this, true))
        {
            UpdateLocalUI();
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
}
