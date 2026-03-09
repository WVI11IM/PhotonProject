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

public enum Sector
{
    Armor = 0,
    Weapon = 1,
    Tank = 2,
    Eject = 3
}

public class InventoryManager : MonoBehaviour
{
    private Dictionary<string, Queue<Item>> inventories =
        new Dictionary<string, Queue<Item>>();

    //Add an item to a UID, appending it at the end and preserving the order
    public void AddItem(string uid, Item item)
    {
        if (!inventories.ContainsKey(uid))
            inventories[uid] = new Queue<Item>();

        inventories[uid].Enqueue(item);
        Debug.Log($"UID {uid}: Added {item.itemType} at position {inventories[uid].Count}");
    }

    //Returns the list of items from a specific UID
    public Queue<Item> GetItems(string uid)
    {
        if (!inventories.ContainsKey(uid))
            inventories[uid] = new Queue<Item>();

        return inventories[uid];
    }

    //Removes item at the front of queue from a specific UID
    public void DeployItem(string uid)
    {
        if (!inventories.ContainsKey(uid))
            inventories[uid] = new Queue<Item>();

        Item deployedItem = inventories[uid].Dequeue();
        Debug.Log($"UID {uid}: Deployed {deployedItem.itemType}");
    }
    //>>>>>>>>>>>>>>>>ASSOCIATE THIS WITH SECTORS LATER<<<<<<<<<<<<<<<<<<<<<<

    //Clears the item list from a specific UID
    public void ClearInventory(string uid)
    {
        if (inventories.ContainsKey(uid))
            inventories[uid].Clear();
    }

    //Clears the item list from all UIDs
    public void ClearAllInventories()
    {
        inventories.Clear();
    }
}
