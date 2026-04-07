using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CargoHoldManager : NetworkBehaviour
{
    private Queue<ItemType> cargoQueue = new Queue<ItemType>();

    [SerializeField] private int queueCapacity = 10;
    [SerializeField] private TMPro.TextMeshProUGUI cargoQueueText;

    [SerializeField] private AudioSource audioNewCargo;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestAddItem(ItemType item)
    {
        TryAddToQueue(item);
    }
    
    //Checks queue capacity for adding an item
    public void TryAddToQueue(ItemType itemType)
    {
        if (!Object.HasStateAuthority)
            return;

        //If queue still has room, adds the item to the queue
        if (cargoQueue.Count < queueCapacity)
        {
            cargoQueue.Enqueue(itemType);

            if (audioNewCargo != null) audioNewCargo.Play();
            Debug.Log($"{itemType} added to local cargo queue");

            UpdateCargoQueueText();
        }
        //Else, it gets ejected and lost forever i guess
        else
        {
            Debug.LogWarning($"[Queue FULL] {itemType} ejected!");
            AutoEjectItem(itemType);
        }
    }

    //For now it's just a log that tells us which item was ejected
    private void AutoEjectItem(ItemType itemType)
    {
        Debug.Log($"Cargo queue is full!!!\n{itemType} was automatically ejected");
    }

    //Tries to get the next item from the queue
    public bool TryGetNextItemFromCargoQueue(out ItemType itemType)
    {
        //If it's empty, return false
        if (cargoQueue == null || cargoQueue.Count == 0)
        {
            itemType = default;
            return false;
        }
        itemType = cargoQueue.Dequeue();
        UpdateCargoQueueText();
        return true;
    }

    //Manually updates cargo queue text
    public void UpdateCargoQueueText()
    {
        if (Object.HasStateAuthority && cargoQueueText != null)
        {
            cargoQueueText.text = GetCargoQueueText();
        }
    }

    //Returns a string of all items on queue
    public string GetCargoQueueText()
    {
        string queueString = "CARGO QUEUE:\n";
        if (cargoQueue.Count > 0)
        {
            foreach (var item in cargoQueue)
            {
                queueString += item.ToString() + " <- ";
            }
            //Trim the last arrow
            queueString = queueString.TrimEnd(' ', '<', '-');
        }
        else
        {
            queueString += "empty";
        }
        return queueString;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestLeechItem()
    {
        TryLeechFromQueue();
    }

    //Checks queue for item leeching
    public void TryLeechFromQueue()
    {
        if (!Object.HasStateAuthority)
            return;

        //If cargo queue has any item
        if (cargoQueue.Count > 0)
        {
            ItemType[] arr = cargoQueue.ToArray();
            ItemType leechedItem = arr[arr.Length - 1]; //newest item
            //had to rebuild the queue without the leeched item
            //since there's no option to dequeue from the opposite side

            Queue<ItemType> newQueue = new Queue<ItemType>();
            for (int i = 0; i < arr.Length - 1; i++)
            {
                newQueue.Enqueue(arr[i]);
            }
            cargoQueue = newQueue;

            Debug.Log($"{leechedItem} was leeched by enemy!!");
            UpdateCargoQueueText();
        }
        else
        {
            Debug.Log("No items to leech");
        }
    }
}
