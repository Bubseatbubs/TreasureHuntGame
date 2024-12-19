using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TCPConnection : MonoBehaviour
{
    private TcpClient host;
    private NetworkStream hostStream;
    public static TCPConnection instance;
    Thread hostThread;

    /* 
    Creates a new TCPClient that connects to a server.
    The client will gain an ID based on what the server sends it back.
    */
    public void Instantiate(string hostIP, int port)
    {
        if (instance)
        {
            Debug.Log("TCPConnection Instance already exists, replacing");
            instance.Delete();
            instance = this;
        }

        // No instance yet, set this to it
        instance = this;

        // Initialize client
        host = new TcpClient(hostIP, port);
        hostStream = host.GetStream();
        Debug.Log("Connected to TCP host at IP: " + hostIP);

        // First message returned is the ID
        byte[] bufferID = new byte[4];
        int bytesRead = hostStream.Read(bufferID, 0, bufferID.Length);
        if (bytesRead == 4)
        {
            int receivedNumber = BitConverter.ToInt32(bufferID, 0);
            Debug.Log("Client's ID is now: " + receivedNumber);
            NetworkController.ID = receivedNumber;
        }
        else
        {
            Debug.Log($"Error: 4 bytes were expected but {bytesRead} bytes were sent.");
        }

        // Begin accepting information from host
        hostThread = new Thread(() => HandleHost(hostStream, 0));
        hostThread.Start();
    }

    /* 
    Disconnects from the host.
    */
    public void DisconnectFromHost()
    {
        SendDataToHost("TCP_TreasureHunt:Disconnect");
        Delete();
    }

    /* 
    Resets the TCPConnection instance for future use.
    */
    public void Delete()
    {
        hostThread?.Abort();
        host?.Close();
        instance = null;
        Destroy(this);
    }

    /* 
    Checks for data from the host.
    Data received from the host is passed to the NetworkController to be parsed.
    */
    private void HandleHost(NetworkStream peerStream, int peerID)
    {
        byte[] peerBuffer = new byte[4096];
        Debug.Log("Awaiting messages from peer");
        try
        {
            while (true)
            {
                int bytesRead = peerStream.Read(peerBuffer, 0, peerBuffer.Length);
                if (bytesRead == 0) break;

                // Read the message sent
                string message = Encoding.UTF8.GetString(peerBuffer, 0, bytesRead);
                Debug.Log($"Received TCP message from peer {peerID}: " + message);

                if (message.Equals("TCP_TreasureHunt:Disconnect"))
                {
                    throw new SocketException();
                }

                // Handle received data
                NetworkController.AddData(message);
            }
        }
        catch (SocketException)
        {
            // If client disconnects or some other error occurs while reading
            Debug.Log($"Connection to host was lost.");
            MainThreadDispatcher.instance.Enqueue(() => NetworkController.instance.DisconnectFromGame());
        }
        catch (ThreadAbortException)
        {
            // If client disconnects while thread was in the middle of running
            Debug.Log($"Connection to host was forcibly closed by client.");
            MainThreadDispatcher.instance.Enqueue(() => NetworkController.instance.DisconnectFromGame());
        }

    }

    /* 
    Sends a message to the host
    */
    public void SendDataToHost(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        hostStream.Write(data, 0, data.Length);
        hostStream.Flush();
    }

    /* 
    Sends a message to the host, stops the HandleHost thread, and blocks until the
    host has replied back. 

    Upon receiving a reply, this method returns the message that it received from
    the host, and resumes the HandleHost thread.
    */
    public string SendAndReceiveDataFromHost(string message)
    {
        // Temporarily stop host thread to get specific data
        hostThread.Abort();
        byte[] peerBuffer = new byte[4096];
        SendDataToHost(message);
        int bytesRead = hostStream.Read(peerBuffer, 0, peerBuffer.Length);
        if (bytesRead == 0)
        {
            Debug.Log("Error reading from host, resending!");
            return SendAndReceiveDataFromHost(message);
        }

        string receivedData = Encoding.UTF8.GetString(peerBuffer, 0, bytesRead);
        Debug.Log($"Received from Host {receivedData}");

        // Restart host thread
        hostThread = new Thread(() => HandleHost(hostStream, 0));
        hostThread.Start();
        return receivedData;
    }

    /* 
    Runs when Unity detects that the game has been quit out of 
    */
    private void OnApplicationQuit()
    {
        DisconnectFromHost();
    }
}
