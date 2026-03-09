using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CargoHoldManager : NetworkBehaviour
{
    private Queue<ItemType> cargoQueue = new Queue<ItemType>();
    [SerializeField] private int queueCapacity = 10;
    [SerializeField] private TMPro.TextMeshProUGUI cargoQueueText;

    public override void Spawned()
    {
        //Only shows cargo queue text if engineer
        if (cargoQueueText != null)
        {
            cargoQueueText.gameObject.SetActive(Object.HasInputAuthority);
        }
    }

    //Checks queue capacity for adding an item
    public void TryAddToQueue(ItemType itemType)
    {
        //If queue still has room, adds the item to the queue
        if (cargoQueue.Count < queueCapacity)
        {
            cargoQueue.Enqueue(itemType);
            Debug.Log($"{itemType} added to cargo queue");
            RPC_UpdateCargoQueueText(GetCargoQueueText());

        }
        //Else, it gets ejected and lost forever i guess
        else
        {
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
        if(cargoQueue.Count == 0)
        {
            itemType = default;
            return false;
        }
        //Else, remove the "oldest" item from queue
        itemType = cargoQueue.Dequeue();
        RPC_UpdateCargoQueueText(GetCargoQueueText());
        Debug.Log($"{itemType} removed from cargo queue");
        return true;
    }

    //RPC for updating the cargo queue text
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_UpdateCargoQueueText(string newText)
    {
        if (cargoQueueText != null)
            cargoQueueText.text = newText;
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
            //Trim the end arrow
            queueString = queueString.TrimEnd(' ', '<', '-');
        }
        else
        {
            queueString += "empty";
        }
        return queueString;
    }
}
