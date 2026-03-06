using Fusion;
using System;
using System.Collections;
using System.IO.Ports;
using System.Text;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Management;


public class RFIDReader : NetworkBehaviour
{
    [Header("Inventory Manager")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Serial Settings")]
    [SerializeField] private int baudRate = 9600;
    [SerializeField] private float reconnectInterval = 2f;

    [Header("UI")]
    [SerializeField] public TMP_Text rfidDisplayText;

    private SerialPort sp;
    private StringBuilder buffer = new StringBuilder();

    public override void Spawned()
    {
        if (rfidDisplayText == null)
        {
            rfidDisplayText = GameObject.Find("RFIDText").GetComponent<TMP_Text>();
        }
    }

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
                Debug.Log("RFID UID: " + uid);

                OnUIDScanned(uid);
                RPC_BroadcastToClients(uid);

            }

        }
    }

    private void OnUIDScanned(string uid)
    {
        if (inventoryManager == null) return;

        //Testing: puts a random item in the scanned inventory
        Item newItem = new Item("Item" + UnityEngine.Random.Range(1, 100));
        inventoryManager.AddItem(uid, newItem);

        var items = inventoryManager.GetItems(uid);
        string sequence = string.Join(" -> ", items.ConvertAll(i => i.itemName));
        Debug.Log($"UID {uid} inventory order: {sequence}");
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_BroadcastToClients(string uid)
    {
        if (rfidDisplayText != null)
        {
            rfidDisplayText.text += $"Card scanned: {uid}\n";
        }
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
