using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UDPLobbyConnection : MonoBehaviour
{
    private UdpClient lobbyClient;
    private IPEndPoint broadcastEndPoint;
    public static UDPLobbyConnection instance;
    private bool isUDPPortActive = false;
    private int port;

    // Singleton
    public void Instantiate(int port)
    {
        if (instance)
        {
            return;
        }

        // No instance yet, set this to it
        instance = this;

        lobbyClient = new UdpClient();
        lobbyClient.EnableBroadcast = true;

        lobbyClient.BeginReceive(OnReceiveData, null);
        this.port = port;
        broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
        isUDPPortActive = true;
    }

    /* 
    Stop broadcasting and delete this instance
    */
    public void Disconnect()
    {
        lobbyClient.EnableBroadcast = false;
        isUDPPortActive = false;
        lobbyClient?.Close();
        instance = null;
        Destroy(this);
    }

    /* 
    Sends a broadcast to ask UDPhosts for the address
    */
    public void RequestAddressFromHosts()
    {
        Debug.Log("Sending request to broadcast");
        AvailableGamesDropdown.instance.ClearItems();
        String command = "UDP_TreasureHunt:RequestAddress";
        byte[] data = Encoding.UTF8.GetBytes(command);
        lobbyClient.Send(data, data.Length, broadcastEndPoint);
    }

    void FixedUpdate()
    {
        if (isUDPPortActive)
        {
            lobbyClient.BeginReceive(OnReceiveData, null);
        }
    }

    /* 
    Adds the lobby information to a list of available games upon receiving
    the command SendConnectionInfo
     */
    void OnReceiveData(IAsyncResult result)
    {
        try
        {
            IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Receive data
            byte[] data = lobbyClient.EndReceive(result, ref senderEndPoint);
            string message = Encoding.UTF8.GetString(data);
            Debug.Log($"Received UDP message: {message}");
            Debug.Log($"Received answer from {senderEndPoint.Address}: {message}");
            
            // Add IP Address to dropdown list
            if (message.Contains("UDP_TreasureHunt:SendConnectionInfo"))
            {
                string[] splitUp = message.Split(':');
                string username = splitUp[2];
                Debug.Log($"Adding {senderEndPoint.Address} who has a username of {username}");
                AvailableGamesDropdown.instance.AddItem($"{username}'s Lobby", senderEndPoint.Address);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
}
