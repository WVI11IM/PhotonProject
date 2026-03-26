using System;
using System.Collections;
using System.IO.Ports;
using System.Text;
using TMPro;
using UnityEngine;
using System.Management;
using System.Linq;

public class RFIDReader : MonoBehaviour
{
    [Header("Inventory Manager")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private CargoHoldManager cargoHoldManager;

    [Header("Engineer scripts")]
    [SerializeField] private EngineerItemSender engineerSender;
    [SerializeField] private EngineerSectorSelector sectorSelector;
    [SerializeField] private EngineerScreenManager screenManager;

    [Header("Serial Settings")]
    [SerializeField] private int baudRate = 9600;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioCollect;
    [SerializeField] private AudioSource audioEmpty;

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

    //When a card is scanned...
    private void OnUIDScanned(string uid)
    {
        if (inventoryManager == null) return;

        //...checks if the engineer's screen is in the Engine Room mode
        if (screenManager != null && screenManager.CurrentScreen == EngineerScreen.EngineRoom)
        {
            //If the mouse is not on top of any sectors, the scanned item won't be deployed and stays in the card and method returns
            if (sectorSelector == null || sectorSelector.CurrentSector == Sector.None)
            {
                Debug.Log("No sector selected.");
                return;
            }

            var items = inventoryManager.GetItems(uid);

            //If the card has no items inside, returns
            if (items.Count == 0)
            {
                audioEmpty.Play();
                Debug.Log($"UID {uid}: Card has no items");
                return;
            }

            //if scanned card contains an item AND was deployed to a sector, sends it back to the pilot
            Item nextItem = items.Peek();
            ItemType itemType = nextItem.itemType;

            inventoryManager.DeployItem(uid);
            engineerSender.SendItemToPilot(itemType, sectorSelector.CurrentSector);

            Debug.Log($"UID {uid}: Deployed {itemType} to {sectorSelector.CurrentSector}");
            return;
        }

        //...checks if the engineer's screen is in the Cargo Hold mode
        if (screenManager != null && screenManager.CurrentScreen == EngineerScreen.CargoHold)
        {
            if (cargoHoldManager == null) return;

            //If there are no items on queue, returns
            if (!cargoHoldManager.TryGetNextItemFromCargoQueue(out ItemType nextType))
            {
                audioEmpty.Play();
                Debug.Log($"UID {uid}: No items in queue to add");
                return;
            }

            //If there's an item on the cargo queue, add it to the inventory of tapped card

            Item nextItemStored = new Item(nextType);
            inventoryManager.AddItem(uid, nextItemStored);
            audioCollect.Play();

            var storedItems = inventoryManager.GetItems(uid);
            string sequence = string.Join(" <- ", storedItems.Select(i => i.itemType.ToString()));

            Debug.Log($"UID {uid} inventory order: {sequence}");
        }
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
