using Fusion;
using Pilot;
using UnityEngine;
public class EngineerItemSender : NetworkBehaviour
{
    [Networked] public NetworkObject PilotObject { get; set; }

    //Method for engineer to deploy item and sector
    public void SendItemToPilot(ItemType item, Sector sector)
    {
        if (!Object.HasInputAuthority) return;

        if (PilotObject == null)
            return;

        var receiver = PilotObject.GetComponent<PilotItemReceiver>();
        if (receiver != null)
        {
            receiver.RPC_RequestReceiveItem(item, sector);
        }
    }

    public void AssignPilot(NetworkObject pilot)
    {
        if (Object.HasStateAuthority)
        {
            PilotObject = pilot;
            Debug.Log("Pilot assigned to engineer");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.InputAuthority)]
    public void RPC_AssignPilot(NetworkObject pilot)
    {
        AssignPilot(pilot);
    }
}