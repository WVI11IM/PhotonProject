using System;
using System.Collections;
using System.IO.Ports;
using System.Text;
using TMPro;
using UnityEngine;
using System.Management;

public class RFIDReader : MonoBehaviour
{
    [Header("Inventory Manager")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private CargoHoldManager cargoHoldManager;

    [Header("Serial Settings")]
    [SerializeField] private int baudRate = 9600;

    [Header("UI")]
    [SerializeField] public TMP_Text rfidDisplayText;

    private SerialPort sp;
    private StringBuilder buffer = new StringBuilder();

    private void TryConnectToRFID()
    {
        //Get all serial ports from computer
        string[] ports = SerialPort.GetPortNames();

        //Checks each port until success
        foreach (string port in ports)
        {
            try
            {
                SerialPort testPort = new SerialPort(port, baudRate);
                testPort.ReadTimeout = 500;
                testPort.Open();

                sp = testPort;
                sp.DiscardInBuffer();

                Debug.Log("Port opened: " + port);
            }
            catch
            {
                //Ignore the ones that fail
            }
        }
    }

    void Start()
    {
        TryConnectToRFID();

        //if (sp != null && sp.IsOpen)
        //    Debug.Log("RFID reader connected");
        //else
        //    Debug.Log("No RFID reader detected");
    }

    void Update()
    {
        if (sp == null || !sp.IsOpen) return;

        while (sp.BytesToRead > 0)
        { 
            buffer.Append((char)sp.ReadByte());
        }

        int newlineIndex;
        while ((newlineIndex = buffer.ToString().IndexOf('\n')) >= 0)
        {
            string uid = buffer.ToString(0, newlineIndex).Trim();
            buffer.Remove(0, newlineIndex + 1);

            if (!string.IsNullOrEmpty(uid))
            {
                OnUIDScanned(uid);
                UpdateUIDText(uid);
            }

        }
    }

    //When a card is scanned, analyzes the CargoHoldManager to check if any items can be found
    private void OnUIDScanned(string uid)
    {
        if (inventoryManager == null || cargoHoldManager == null) return;

        if (!cargoHoldManager.TryGetNextItemFromCargoQueue(out ItemType nextType))
        {
            Debug.Log($"UID {uid}: No items in queue to add");
            return;
        }

        //If there's an item on the cargo queue, add it to the inventory of tapped card
        Item nextItem = new Item(nextType);
        inventoryManager.AddItem(uid, nextItem);

        var items = inventoryManager.GetItems(uid);
        string sequence = string.Join(" <- ", items.ConvertAll(i => i.itemType.ToString()));
        Debug.Log($"UID {uid} inventory order: {sequence}");
    }

    //Updates text and shows which card got scanned
    private void UpdateUIDText(string uid)
    {
        if (rfidDisplayText != null)
            rfidDisplayText.text += $"Card scanned: {uid}\n";
    }

    void OnDisable()
    {
        ClosePort();
    }

    void OnApplicationQuit()
    {
        ClosePort();
    }

    private void ClosePort()
    {
        if (sp != null && sp.IsOpen)
        {
            sp.Close();
            sp.Dispose();
        }
    }
}
