using Fusion;
using UnityEngine;
public class EngineerItemSender : NetworkBehaviour
{
    [Networked] public NetworkObject PilotObject { get; set; }

    //Method for engineer to deploy item and sector
    public void SendItemToPilot(ItemType item, Sector sector)
    {
        if (!Object.HasInputAuthority) return;

        RPC_SendItemToPilot(item, sector);
    }

    //RPC for engineer to send item and sector back to pilot
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SendItemToPilot(ItemType item, Sector sector)
    {
        if (PilotObject == null)
        {
            Debug.LogWarning("Pilot not assigned");
            return;
        }

        var receiver = PilotObject.GetComponent<PilotItemReceiver>();
        if (receiver != null)
        {
            receiver.ReceiveItem(item, sector);
        }
    }
}