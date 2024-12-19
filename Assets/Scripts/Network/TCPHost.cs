using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TCPHost : MonoBehaviour
{
    private TcpListener listener;
    private Dictionary<int, TcpClient> connectedPeers = new Dictionary<int, TcpClient>();
    private Dictionary<int, NetworkStream> streams = new Dictionary<int, NetworkStream>();
    private byte[] inputBuffer = new byte[1024];
    private Thread listenerThread;
    private int nextID = 0;
    public static TCPHost instance;

    /* 
    Begins a lobby as the host client in the P2P connection.
    Host client acts similar to a server.
    Handles keeping all clients synchronized.
    Host's ID is always 0.
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
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Debug.Log("Hosting a TCP port");

            // Make new thread that handles accepting client connections
            listenerThread = new Thread(() => AcceptClientConnections());
            listenerThread.Start();
        }
        catch (SocketException)
        {
            Debug.Log("You are already hosting on another instance!");
        }
    }

    /* 
    Sends a message to all connected clients
    */
    public void SendDataToClients(string message)
    {
        inputBuffer = Encoding.UTF8.GetBytes(message);
        foreach (KeyValuePair<int, NetworkStream> stream in streams)
        {
            stream.Value.Write(inputBuffer, 0, inputBuffer.Length);
            stream.Value.Flush(); // Ensure it is sent immediately
        }
    }

    /* 
    Sends a message to all connected clients, except one. This can be specified
    by setting the ignoreID.
    */
    public void SendDataToClients(string message, int ignoreID)
    {
        inputBuffer = Encoding.UTF8.GetBytes(message);
        foreach (KeyValuePair<int, NetworkStream> stream in streams)
        {
            if (stream.Key == ignoreID) continue; // Don't send to same client
            try
            {
                stream.Value.Write(inputBuffer, 0, inputBuffer.Length);
                stream.Value.Flush();
            }
            catch (SocketException)
            {
                RemoveClient(stream.Key);
            }


        }
    }

    /* 
    Removes a client from the connection list.
    */
    public void RemoveClient(int ID)
    {
        connectedPeers.Remove(ID);
        streams.Remove(ID);
        Debug.Log($"Removed client {ID}'s TCP connection");
    }

    /* 
    A thread that constantly accepts new client connections as they come in.
    */
    private void AcceptClientConnections()
    {
        Debug.Log("Preparing to accept new connections");
        while (true)
        {
            // Accept the connection
            TcpClient newClient = listener.AcceptTcpClient();
            Debug.Log("Peer connected to TCP Port!");

            InitializeClient(newClient);
        }
    }

    /* 
    Initializes a client to the system by giving them an ID, starting a
    thread to receive their messages from, and add them to the list of
    connections.
    */
    private void InitializeClient(TcpClient peerClient)
    {
        NetworkStream peerStream = peerClient.GetStream();

        // Assign an ID to the client
        nextID++;
        MainThreadDispatcher.instance.Enqueue(() =>
        InitializeClientID(peerStream));
        Debug.Log($"Wrote client ID {nextID}");

        // Add to list of client connections
        connectedPeers.Add(nextID, peerClient);
        streams.Add(nextID, peerStream);

        // Spin a new thread that constantly updates using the peer's data
        Thread clientThread = new Thread(() => HandlePeer(peerStream, nextID));
        clientThread.Start();
    }

    private void InitializeClientID(NetworkStream peerStream)
    {
        inputBuffer = BitConverter.GetBytes(nextID);
        peerStream.Write(inputBuffer, 0, inputBuffer.Length);
        peerStream.Flush();
    }

    /* 
    A thread created for each connection. Handles messages received by a specific
    client and passes their messages to the NetworkController.
    */
    private void HandlePeer(NetworkStream peerStream, int peerID)
    {
        byte[] peerBuffer = new byte[4096];
        Debug.Log($"Awaiting messages from peer {peerID}");
        try
        {
            while (true)
            {
                int bytesRead = peerStream.Read(peerBuffer, 0, peerBuffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(peerBuffer, 0, bytesRead);
                // Debug.Log($"Received from peer {peerID}: " + message);

                if (message.Equals("TCP_TreasureHunt:Disconnect"))
                {
                    throw new Exception();
                }

                NetworkController.AddData(message);
            }
        }
        catch (Exception e)
        {
            // If client disconnects or some other error occurs while reading
            Debug.Log($"Client {peerID} disconnected because of: \"{e.Message}\"");
            RemoveClient(peerID);
            NetworkController.RemovePlayer(peerID);
        }
    }

    /* 
    Disconnects the host from all other clients and destroys this instance.
    */
    public void Disconnect()
    {
        nextID = 0;
        SendDataToClients("TCP_TreasureHunt:Disconnect");
        listenerThread.Abort();
        listener?.Stop();
        instance = null;
        Destroy(this);
    }

    /* 
    Runs when Unity detects that the game has been quit out of 
    */
    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
