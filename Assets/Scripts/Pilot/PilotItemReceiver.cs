using Fusion;
using TMPro;
using UnityEngine;

public class PilotItemReceiver : NetworkBehaviour
{
    [SerializeField] private TMP_Text shipStatusText;

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
            message = $"Item {item} ejected safely";
        }
        //But if an item is placed inside a wrong sector, it gives a penalty to the ship
        else if (!IsItemValidForSector(item, sector))
        {
            message = $"Wrong item deployed! {item} to {sector}! Penalty!";
            TriggerPenalty();
        }
        else
        {
            //The item was deployed correctly
            switch (item)
            {
                case ItemType.Metal: message = AddToArmor(); break;
                case ItemType.Ammo: message = AddToWeapon(); break;
                case ItemType.Fuel: message = AddToTank(); break;
                case ItemType.Debris: message = "Item Debris ejected safely!"; break;
                default: break;
            }
        }

        //For now, it only sends a message to the pilot informing them about the deployed items
        RPC_UpdateShipStatus(message);
    }

    //Checks if item and sector match
    private bool IsItemValidForSector(ItemType item, Sector sector)
    {
        switch (sector)
        {
            case Sector.Armor: return item == ItemType.Metal;
            case Sector.Weapon: return item == ItemType.Ammo;
            case Sector.Tank: return item == ItemType.Fuel;
            case Sector.Eject: return true;
            default: return false;
        }
    }

    string AddToArmor()
    {
        return "Armor Up!";
    }

    string AddToWeapon()
    {
        return "Weapon Up!";
    }

    string AddToTank()
    {
        return "Tank Up!";
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateShipStatus(string message)
    {
        if (shipStatusText != null)
        {
            shipStatusText.text = "SHIP STATUS:\n" + message;
        }
    }

    private void TriggerPenalty()
    {
        //Reduce health, armor or something else
    }
}