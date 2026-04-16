using Fusion;
using Misc;
using UnityEngine;

public class NetworkGameOver : NetworkBehaviour
{
    [SerializeField] private MonoBehaviour[] disableScriptsOnGameOver;
    [SerializeField] private GameObject[] disableGameObjectsOnGameOver;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Pilot.Ship.ShipCore.OnShipDied += HandleShipDied;
        }
    }

    private void OnDestroy()
    {
        if (Object && Object.HasStateAuthority)
        {
            Pilot.Ship.ShipCore.OnShipDied -= HandleShipDied;
        }
    }

    private void HandleShipDied()
    {
        if (!Object.HasStateAuthority)
            return;

        RPC_NotifyGameOver();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotifyGameOver()
    {
        Debug.Log("GAME OVER RECEIVED!!");
        HandleGameOver();
    }

    private void HandleGameOver()
    {
        GameState.IsGameOver = true;

        GameOver.Show();
        foreach (var s in disableScriptsOnGameOver)
        {
            s.enabled= false;
        }
        foreach (var g in disableGameObjectsOnGameOver)
        {
            g.SetActive(false);
        }
    }
}
