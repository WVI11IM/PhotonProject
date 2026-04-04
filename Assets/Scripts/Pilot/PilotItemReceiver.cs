using Fusion;
using TMPro;
using UnityEngine;

public class PilotItemReceiver : NetworkBehaviour
{
    [SerializeField] private TMP_Text shipStatusText;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioRight;
    [SerializeField] private AudioSource audioWrong;
    [SerializeField] private AudioSource audioEject;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestReceiveItem(ItemType item, Sector sector)
    {
        ReceiveItem(item, sector);
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
        else if (!IsItemValidForSector(item, sector))
        {
            audioWrong.Play();
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
        UpdateShipStatusUI(message);
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
        audioRight.Play();
        return "Armor Up!";
    }

    string AddToWeapon()
    {
        audioRight.Play();
        return "Weapon Up!";
    }

    string AddToTank()
    {
        audioRight.Play();
        return "Tank Up!";
    }

    private void UpdateShipStatusUI(string message)
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