using Fusion;
using TMPro;
using UnityEngine;

public class PilotItemSender : NetworkBehaviour
{
    [Networked] public NetworkObject EngineerObject { get; private set; }

    //Method for pilot's UI buttons (at least for testing at the moment)
    public void SendItem(int itemIndex)
    {
        if (!Object.HasInputAuthority) return;

        if (EngineerObject == null)
        {
            Debug.LogWarning("Engineer object not assigned yet");
            return;
        }

        ItemType itemToSend = (ItemType)itemIndex;

        var engineerCargo = EngineerObject.GetComponent<CargoHoldManager>();
        if (engineerCargo != null)
        {
            engineerCargo.RPC_RequestAddItem(itemToSend);
        }
    }

    //Method for assigning engineer to pilot on spawn
    public void AssignEngineer(NetworkObject engineer)
    {
        if (Object.HasStateAuthority)
        {
            EngineerObject = engineer;
            Debug.Log("Engineer assigned to pilot");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.InputAuthority)]
    public void RPC_AssignEngineer(NetworkObject engineer)
    {
        AssignEngineer(engineer);
    }
}
