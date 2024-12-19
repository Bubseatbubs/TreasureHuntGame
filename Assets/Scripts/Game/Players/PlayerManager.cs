using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using Newtonsoft.Json;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public float balance;
}

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    public static PlayerManager instance;
    private static Player playerController;
    private static Vector2 spawnPosition = Vector2.zero;
    private static float correctionThreshold = 0.1f;
    private static Dictionary<int, Player> players = new Dictionary<int, Player>();
    private static Player clientPlayer;
    Vector2 Input;
    Vector2 LastInput;
    private string filePath; // Path to save the JSON file

    [SerializeField]
    private LayerMask playersCanPickUpThisLayer;

    /*
    Singleton Pattern: Make sure there's only one PlayerManager
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
        playerController = playerPrefab.GetComponent<Player>();
        filePath = Path.Combine(Application.persistentDataPath, "scoreboard.json");
    }

    void FixedUpdate()
    {
        if (IsPlayerInitialized())
        {
            // Tell other clients to update player's position
            // Calls NetworkController to handle sending data to other players
            Input = clientPlayer.GetInput();
            if (Input != LastInput)
            {
                // Only send an input if need be
                if (NetworkController.isHost)
                {
                    UDPHost.instance.SendDataToClients(SendPlayerInput());
                }
                else
                {
                    UDPConnection.instance.SendDataToHost(SendPlayerInput());
                }
            }

            LastInput = Input;
        }
    }

    public static void CreateNewPlayer(int id, String username)
    {
        if (players.ContainsKey(id))
        {
            // Player already exists
            Debug.Log($"A player with ID {id} already exists.");
            return;
        }
        else
        {
            // Default player name
            if (username.Equals(""))
            {
                username = $"Player {id}";
            }

            // Instantiate a new player
            Player player = Instantiate(playerController, spawnPosition, Quaternion.identity);
            player.AssignID(id);
            player.AssignUsername(username);
            players.Add(id, player);
            Debug.Log($"Created player with ID of {id} and username {username}");
            if (!IsPlayerInitialized())
            {
                clientPlayer = player;
                Debug.Log($"Player {id} is now client's player!");
            }
        }
    }

    public string SendPlayerPosition()
    {
        string response = "PlayerManager:UpdatePlayerPosition" + clientPlayer.ID + ":" +
        clientPlayer.GetXPosition().ToString() + ":" +
        clientPlayer.GetYPosition().ToString();
        return response;
    }

    public void SendPlayerPositions()
    {
        string response = "PlayerManager:UpdatePlayerPositions:";
        foreach (KeyValuePair<int, Player> p in players)
        {
            response += p.Key + "|" + p.Value.GetXPosition() + "|" +
            p.Value.GetYPosition() + "/";
        }

        UDPHost.instance.SendDataToClients(response);
    }

    public static void UpdatePlayerPosition(int id, float x, float y)
    {
        if (!players.ContainsKey(id))
        {
            Debug.Log($"Adding player with ID {id}");
            CreateNewPlayer(id, $"Player {id}");
        }

        Vector2 currentPosition = players[id].GetPosition();
        Vector2 serverPosition = new Vector2(x, y);

        if (Vector2.Distance(currentPosition, serverPosition) > correctionThreshold)
        {
            players[id].SetPosition(Vector2.Lerp(currentPosition, serverPosition, 0.25f));
        }
        else
        {
            players[id].SetPosition(Vector2.Lerp(currentPosition, serverPosition, 0.05f));
        }
    }

    public static void UpdatePlayerPositions(string message)
    {
        string[] playerData = message.Split('/');
        for (int i = 0; i < playerData.Length - 1; i++)
        {
            try
            {
                string[] currentPlayerData = playerData[i].Split('|');
                int playerId = int.Parse(currentPlayerData[0]);
                float x = float.Parse(currentPlayerData[1]);
                float y = float.Parse(currentPlayerData[2]);
                UpdatePlayerPosition(playerId, x, y);
            }
            catch (Exception)
            {
                Debug.LogWarning($"Couldn't parse {playerData[i]}");
            }
        }
    }

    public void BeginSendingHostPositionsToClients()
    {
        InvokeRepeating("SendHostPositionsToClients", 0f, 0.1f);
    }

    private void SendHostPositionsToClients()
    {
        // Send player movement data to clients
        if (UDPHost.instance != null)
        {
            SendPlayerPositions();
        }
    }

    // Asks the host to send the usernames of all players to the client
    public static void RequestPlayerUsernames()
    {
        TCPConnection.instance.SendDataToHost("PlayerManager:SendPlayerUsernames");
    }

    public static void SendPlayerUsernames()
    {
        string response = "PlayerManager:UpdatePlayerUsernames:";
        foreach (KeyValuePair<int, Player> p in players)
        {
            response += p.Key + "|" + p.Value.username + "/";
        }

        TCPHost.instance.SendDataToClients(response);
    }

    public static void UpdatePlayerUsernames(string message)
    {
        string[] playerData = message.Split('/');
        for (int i = 0; i < playerData.Length - 1; i++)
        {
            string[] currentPlayerData = playerData[i].Split('|');
            int playerID = int.Parse(currentPlayerData[0]);
            string username = currentPlayerData[1];

            if (!players.ContainsKey(playerID))
            {
                Debug.Log($"Adding player with playerID {playerID}");
                CreateNewPlayer(playerID, $"Player {playerID}");
            }

            players[playerID].AssignUsername(username);
            Debug.Log($"Set player {playerID} to {username}");
        }
    }

    public static void CheckForItemPickup(int playerID)
    {
        Vector2 playerPos = players[playerID].GetPosition();
        Item itemToPickup = GetClosestItem(playerPos, 1.5f);
        if (itemToPickup == null)
        {
            Debug.Log($"No items nearby for player {playerID} to pickup!");
            return;
        }

        // Host
        int itemID = itemToPickup.ID;
        UpdateItemPickup(playerID, itemID);
        TCPHost.instance.SendDataToClients($"PlayerManager:UpdateItemPickup:{playerID}:{itemID}");
    }

    public static void UpdateItemPickup(int playerID, int itemID)
    {
        players[playerID].AddItem(itemID);

        ItemManager.StartItemPickupAnimation(itemID, players[playerID]);
        ItemManager.HideItem(itemID);
    }

    static Item GetClosestItem(Vector2 playerPos, float radius)
    {
        Item closestItem = null;
        Collider2D[] items = Physics2D.OverlapCircleAll(playerPos, radius, instance.playersCanPickUpThisLayer);
        if (items.Length == 0)
        {
            return closestItem;
        }

        float minimumDistance = Mathf.Infinity;
        Vector2 currentPos = playerPos;
        foreach (Collider2D item in items)
        {
            float distance = Vector2.Distance(item.transform.position, currentPos);
            if (distance < minimumDistance)
            {
                closestItem = item.gameObject.GetComponent<Item>();
                minimumDistance = distance;
            }
        }

        return closestItem;
    }

    public static void DropPlayerItem(int playerID)
    {
        players[playerID].DropItem();
        if (NetworkController.isHost)
        {
            UDPHost.instance.SendDataToClients($"PlayerManager:DropPlayerItem:{playerID}");
        }
    }

    public static void ReturnPlayerItems(int playerID)
    {
        players[playerID].ReturnItems();
    }

    public string SendPlayerInput()
    {
        Vector2 Input = clientPlayer.GetInput();
        string response = "PlayerManager:ReceivePlayerInput:" + clientPlayer.ID + ":" +
        Input.x + ":" + Input.y;
        return response;
    }

    public static void ReceivePlayerInput(int id, float x, float y)
    {
        Vector2 Input = new Vector2(x, y).normalized;
        players[id].SetInput(Input);
    }

    public Player GetHighestScoringPlayer()
    {
        double highestBalance = int.MinValue;
        int id = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].balance > highestBalance)
            {
                highestBalance = players[i].balance;
                id = i;
            }
        }

        return players[id];
    }

    public void UpdateScoreboard()
    {
        // Initialize list
        List<PlayerData> playerScores = new List<PlayerData>();

        if (File.Exists(filePath))
        {
            // Scoreboard file already exists, add all existing players to the list to sort them alongside new entrants
            string existingPlayers = File.ReadAllText(filePath);
            Debug.Log("Found existing scoreboard file" + existingPlayers);
            try {
                playerScores = JsonConvert.DeserializeObject<List<PlayerData>>(existingPlayers);
            } catch (Exception) {
                Debug.LogWarning("Could not read scoreboard file!");
            }
            
        }

        foreach (KeyValuePair<int, Player> player in players)
        {
            try
            {
                // Add player name and balance to a serializable field
                string username = player.Value.username;
                float currentBalance = (float) player.Value.balance;
                PlayerData curPlayerData = new PlayerData {
                    playerName = username,
                    balance = currentBalance
                };

                Debug.Log($"Added player {curPlayerData.playerName} to the scoreboard");
                playerScores.Add(curPlayerData);
            }
            catch (NullReferenceException)
            {
                Debug.Log("Couldn't find player");
            }
        }

        playerScores.Sort((x, y) => y.balance.CompareTo(x.balance));

        // Serialize the object to JSON
        string json = JsonConvert.SerializeObject(playerScores, Formatting.Indented);

        // Write the JSON string to a file
        File.WriteAllText(filePath, json);

        ChatManager.CreateMessageInChat($"Updated Scoreboard.json, located in {filePath}");
    }

    public void RemovePlayer(int ID)
    {
        string username = players[ID].username;
        if (NetworkController.isHost)
        {
            MainThreadDispatcher.instance.Enqueue(() => ChatManager.instance.SendSystemMessage($"{username} left the game"));
        }

        Player curPlayer = players[ID];
        players.Remove(ID);
        MainThreadDispatcher.instance.Enqueue(() => curPlayer.Delete());
    }

    public static void KillPlayer(int ID)
    {
        players[ID].Kill();
    }

    public static Boolean IsPlayerInitialized()
    {
        return clientPlayer != null;
    }

    public void Reset()
    {
        CancelInvoke();
        clientPlayer = null;
        foreach (KeyValuePair<int, Player> player in players)
        {
            try
            {
                player.Value.Delete();
            }
            catch (NullReferenceException)
            {
                Debug.Log("Couldn't find player");
            }
        }

        players.Clear();
    }
}