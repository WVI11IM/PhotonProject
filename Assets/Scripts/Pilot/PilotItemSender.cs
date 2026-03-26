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

        RPC_SendItem(EngineerObject, itemToSend);
    }

    //RPC for pilot to try and send an item to the engineer
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SendItem(NetworkObject engineer, ItemType item)
    {
        var chm = engineer.GetComponent<CargoHoldManager>();
        chm.TryAddToQueue(item);
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
}
