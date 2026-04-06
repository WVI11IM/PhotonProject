using Fusion;
using UnityEngine;

namespace Pilot
{

    public class PilotItemSender : NetworkBehaviour
    {

        private static PilotItemSender _instance;
        public static PilotItemSender Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<PilotItemSender>();
                return _instance;
            }
        }

        [Networked] public NetworkObject EngineerObject { get; private set; }

        //Method for pilot's UI buttons (at least for testing at the moment)
        public void SendItem(int itemIndex) => SendItem((ItemType)itemIndex);
        public void SendItem(ItemType itemType)
        {
            if (!Object.HasInputAuthority) return;

            if (EngineerObject == null)
            {
                Debug.LogWarning("Engineer object not assigned yet");
                return;
            }

            RPC_SendItem(EngineerObject, itemType);
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
}
