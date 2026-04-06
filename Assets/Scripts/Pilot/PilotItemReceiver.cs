using Fusion;
using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Pilot {

    public class PilotItemReceiver : NetworkBehaviour
    {
        
        private static PilotItemReceiver _instance;
        public static PilotItemReceiver Instance {
            get {
                if (_instance == null)
                    _instance = FindAnyObjectByType<PilotItemReceiver>();
                return _instance;
            }
        }
        
        [SerializeField] private TMP_Text shipStatusText;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource audioRight;
        [SerializeField] private AudioSource audioWrong;
        [SerializeField] private AudioSource audioEject;
        private string _status;

        public override void Spawned()
        {
            //When spawned, looks for the engineer in order to assign itself to their EngineerItemSender component
            GameObject engineerGO = GameObject.FindWithTag("Engineer");
            if (engineerGO != null)
            {
                EngineerItemSender sender = engineerGO.GetComponent<EngineerItemSender>();
                if (sender != null && sender.Object.HasStateAuthority)
                {
                    sender.PilotObject = this.Object;
                    Debug.Log("Pilot registered itself to engineer");
                }
            }
        }

        //Receives the item and sector from the engineer
        public void ReceiveItem(ItemType item, Sector sector)
        {
            if (!Object.HasStateAuthority) return;

            Debug.Log($"Pilot received deployed item: {item}");

            string message = "";

            //If it's the eject sector, no penalty is applied
            if (sector == Sector.Eject)
            {
                audioEject.Play();
                message = $"Item {item} ejected safely";
            }
            //But if an item is placed inside a wrong sector, it gives a penalty to the ship
            else if ((int)item != (int)sector)
            {
                audioWrong.Play();
                message = $"Wrong item deployed! {item} to {sector}! Penalty!";
                ShipStats.Instance.IncorrectSectorPenalty((ItemType)sector);
            }
            else
            {
                //The item was deployed correctly
                ShipStats.Instance.ReplenishResource(item);
            }

            //For now, it only sends a message to the pilot informing them about the deployed items
            RPC_UpdateShipStatus(message);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateShipStatus(string message) {
            _status = message;
        }
    }

}