using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public static string chatText;
    public static ChatManager instance;
    private string username;

    /*
    Singleton Pattern: Make sure there's only one Chat Window
    */
    void Awake()
    {
        if (instance)
        {
            // Remove if instance exists
            Destroy(gameObject);
            return;
        }

        // No instance yet, set this to it
        instance = this;
    }

    public void SendChatMessage(string message)
    {
        if (username == null)
        {
            username = NetworkController.instance.username;
        }

        if (message == "") {
            return;
        }

        if (NetworkController.isHost)
        {
            Debug.Log("Sent as host");
            CreateMessageInChat($"{username} — {message}");
        }
        else
        {
            Debug.Log("Sent as client");
            TCPConnection.instance.SendDataToHost($"ChatManager:CreateMessageInChat:{username} — {message}");
        }
    }

    public void SendSystemMessage(string message)
    {
        if (NetworkController.isHost)
        {
            Debug.Log("Sent as host");
            CreateMessageInChat($"{message}");
        }
        else
        {
            Debug.Log("Sent as client");
            TCPConnection.instance.SendDataToHost($"ChatManager:CreateMessageInChat:{message}");
        }
    }

    public static void CreateMessageInChat(string message)
    {
        Debug.Log("Writing chat message");
        chatText += message + "\n";
        ChatMessagesDisplay.instance.UpdateChatMessages(chatText);
        if (NetworkController.isHost)
        {
            // Use TCP to ensure the message is reliably sent
            Debug.Log("Writing to clients");
            TCPHost.instance.SendDataToClients($"ChatManager:CreateMessageInChat:{message}");
        }
    }
}