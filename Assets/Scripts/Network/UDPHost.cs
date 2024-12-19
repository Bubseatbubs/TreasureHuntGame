using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using UnityEngine;

public class UDPHost : MonoBehaviour
{
    public static UDPHost instance;
    private UdpClient udpServer;
    private HashSet<IPEndPoint> connectedClients;
    private IPEndPoint broadcastEndPoint;
    private bool isUDPPortActive;
    private bool isUDPPortBroadcasting;

    /* 
    Begins a UDP host.
    UDP host is in charge of sending constant game updates to players.
    Additionally, player updates that would require constant updates like
    player movement are handled by the UDP host.
    */
    public void Instantiate(int port)
    {
        try
        {
            if (instance)
            {
                return;
            }

            // No instance yet, set this to it
            instance = this;

            // Begin listening for other peers
            udpServer = new UdpClient(port);
            connectedClients = new HashSet<IPEndPoint>();

            udpServer.BeginReceive(OnReceiveData, null);
            udpServer.EnableBroadcast = true;
            broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            Debug.Log("Hosting a UDP port");

            isUDPPortActive = true;
            isUDPPortBroadcasting = true;
        }
        catch (SocketException)
        {
            Debug.Log("You are already hosting on another instance!");
        }
    }

    /*
    Hands data received from a client to the NetworkController.
    */
    private void OnReceiveData(IAsyncResult result)
    {
        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = udpServer.EndReceive(result, ref clientEndPoint);

        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"Received UDP message: {message}");

        if (message.Equals("UDP_TreasureHunt:RequestAddress") && isUDPPortBroadcasting)
        {
            // A client is broadcasting and asking for address
            // Send address back on broadcast
            Debug.Log("Broadcast message received, sending back address");
            String command = $"UDP_TreasureHunt:SendConnectionInfo:{NetworkController.instance.username}";
            byte[] broadcastData = Encoding.UTF8.GetBytes(command);
            udpServer.Send(broadcastData, broadcastData.Length, clientEndPoint);
            return;
        }
        else if (message.Contains("UDP_TreasureHunt:SendConnectionInfo"))
        {
            // Received either own connection info or other hosts, throw away
            return;
        }
        else if (message.Equals("UDP_TreasureHunt:Connect") || !connectedClients.Contains(clientEndPoint))
        {
            Debug.Log($"Added client {clientEndPoint} to endpoint list");
            connectedClients.Add(clientEndPoint);
            return;
        }
        else if (message.Equals("UDP_TreasureHunt:Disconnect"))
        {
            Debug.Log($"Removing client {clientEndPoint} from endpoint list");
            RemoveClient(clientEndPoint);
            return;
        }

        NetworkController.AddData(message);
        SendDataToClients(message);
    }

    /* 
    Sends data to the UDP client.
    */
    public void SendDataToClients(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        foreach (var client in connectedClients)
        {
            udpServer.Send(data, data.Length, client);
            // Debug.Log($"Sent to {client}: {message}");
        }
    }

    /* 
    Removes a client from the connection list.
    */
    public void RemoveClient(IPEndPoint clientEndPoint)
    {
        connectedClients.Remove(clientEndPoint);
    }

    /* 
    Destroys this instance.
    */
    public void Disconnect()
    {
        isUDPPortActive = false;
        udpServer.Close();
        instance = null;
        Destroy(this);
    }

    public void StopBroadcasting()
    {
        isUDPPortBroadcasting = false;
    }

    void FixedUpdate()
    {
        // If the UDP socket is running, check for new data from the socket
        if (isUDPPortActive)
        {
            udpServer.BeginReceive(OnReceiveData, null);
        }
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
}
