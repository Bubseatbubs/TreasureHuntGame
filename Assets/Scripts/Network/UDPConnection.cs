using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UDPConnection : MonoBehaviour
{
    private UdpClient client;
    private IPEndPoint serverEndPoint;
    public static UDPConnection instance;
    private bool isUDPPortActive = false;

    /* 
    Creates a new UDPClient that connects to the server.
    */
    public void Instantiate(string hostIP, int port)
    {
        if (instance)
        {
            return;
        }

        // No instance yet, set this to it
        instance = this;

        client = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse(hostIP), port);

        Debug.Log($"Connected to UDP port: {hostIP}");
        SendDataToHost("UDP_TreasureHunt:Connect");
        client.BeginReceive(OnReceiveData, null);
        isUDPPortActive = true;
    }

    /* 
    Resets the UDPConnection instance for future use.
    */
    public void Delete()
    {
        client?.Close();
        instance = null;
        Destroy(this);
    }

    /* 
    Sends data to the UDP host.
    */
    public void SendDataToHost(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        // Debug.Log($"Sent UDP: {message}");
        client.Send(data, data.Length, serverEndPoint);
    }

    /*
    Hands data received from a host to the NetworkController.
    */
    private void OnReceiveData(IAsyncResult result)
    {
        byte[] data = client.EndReceive(result, ref serverEndPoint);
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"Received UDP: {message}");
        NetworkController.AddData(message);
    }

    /*
    Runs every frame by Unity. 
    */
    void FixedUpdate()
    {
        // If the UDP socket is running, check for new data from the socket
        if (isUDPPortActive)
        {
            client.BeginReceive(OnReceiveData, null);
        }
    }

    /* 
    Runs when Unity detects that the game has been quit out of 
    */
    void OnApplicationQuit()
    {
        Delete();
    }
}
