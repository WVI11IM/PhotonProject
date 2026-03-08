using UnityEngine;
using Fusion;

public class PilotItemSender : NetworkBehaviour
{
    [Networked] public NetworkObject EngineerObject { get; private set; }

    public void AssignEngineer(NetworkObject engineer)
    {
        if (Object.HasStateAuthority)
        {
            EngineerObject = engineer;
            Debug.Log("Engineer assigned to pilot");
        }
    }

    public void SendItem(int itemIndex)
    {
        if (!Object.HasInputAuthority) return;

        if (EngineerObject == null)
        {
            Debug.LogWarning("Engineer object not assigned yet");
            return;
        }

        ItemType itemToSend = (ItemType)itemIndex;

        RPC_SendItem(EngineerObject, itemToSend);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SendItem(NetworkObject engineer, ItemType item)
    {
        var chm = engineer.GetComponent<CargoHoldManager>();
        chm.TryAddToQueue(item);
    }
}
