using Fusion;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Metal = 0,
    Ammo = 1,
    Fuel = 2,
    Debris = 3
}

public class InventoryManager : NetworkBehaviour
{
    private Dictionary<string, List<Item>> inventories =
        new Dictionary<string, List<Item>>();

    //Add an item to a UID, appending it at the end and preserving the order
    public void AddItem(string uid, Item item)
    {
        if (!inventories.ContainsKey(uid))
            inventories[uid] = new List<Item>();

        inventories[uid].Add(item);
        Debug.Log($"UID {uid}: Added {item.itemType} at position {inventories[uid].Count}");
    }

    //Returns the list of items from a UID
    public List<Item> GetItems(string uid)
    {
        if (!inventories.ContainsKey(uid))
            inventories[uid] = new List<Item>();

        return inventories[uid];
    }

    //Clears the item list from a UID
    public void ClearInventory(string uid)
    {
        if (inventories.ContainsKey(uid))
            inventories[uid].Clear();
    }
}
