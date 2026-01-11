using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class UDPReceiver : MonoBehaviour
{
    public int port = 12345;
    public TextMeshProUGUI heartRateText; // Assign in Inspector

    private UdpClient udpClient;
    private Thread receiveThread;
    private string lastReceivedData = "No Data";

    private static int pendingHeartRate = -1;
    private static readonly object lockObj = new object();

    public delegate void HeartRateReceived(int heartRate);
    public static event HeartRateReceived OnHeartRateReceived;

    void Start()
    {
        try
        {
            udpClient = new UdpClient(port);
            receiveThread = new Thread(ReceiveData)
            {
                IsBackground = true
            };
            receiveThread.Start();
            Debug.Log("UDP Receiver started on port " + port);
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.Message);
        }
    }

    void ReceiveData()
    {
        try
        {
            while (udpClient != null)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
                byte[] data = udpClient.Receive(ref remoteEP);
                string message = Encoding.UTF8.GetString(data);
                Debug.Log("Received UDP message: " + message);

                if (int.TryParse(message, out int hr))
                {
                    lock (lockObj)
                    {
                        pendingHeartRate = hr;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("UDP Receive error: " + e.ToString());
        }
    }

    void Update()
    {
        int hrToProcess = -1;

        lock (lockObj)
        {
            if (pendingHeartRate != -1)
            {
                hrToProcess = pendingHeartRate;
                pendingHeartRate = -1;
            }
        }

        if (hrToProcess != -1)
        {
            lastReceivedData = hrToProcess.ToString();
            OnHeartRateReceived?.Invoke(hrToProcess);
        }

        if (heartRateText != null)
        {
            heartRateText.text = "Heart Rate: " + lastReceivedData + " BPM";
        }
    }

    void OnApplicationQuit() => CloseSocket();
    void OnDestroy() => CloseSocket();

    private void CloseSocket()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        udpClient?.Close();
        udpClient = null;
    }
}
