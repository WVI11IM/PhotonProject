using Fusion;
using System;
using System.Collections;
using System.IO.Ports;
using System.Text;
using UnityEngine;
using TMPro;

public class RFIDReader : NetworkBehaviour
{
    [Header("Serial Settings")]
    [SerializeField] private string portName = "COM3";
    [SerializeField] private int baudRate = 9600;

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

    void Start()
    {
        if (!Object.HasStateAuthority) return;

        sp = new SerialPort(portName, baudRate);
        sp.Open();
        sp.ReadTimeout = 1000;
        sp.DiscardInBuffer();
    }

    void Update()
    {
        if (!Object.HasStateAuthority || sp == null || !sp.IsOpen) return;

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

                if (Object.HasStateAuthority)
                {
                    RPC_BroadcastToClients(uid);
                }
                    
            }

        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
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
