using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    private static Enemy enemyTemplate;
    public static Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();
    public static EnemyManager instance;
    private static int nextEnemyID = 0;

    /*
    Singleton Pattern: Make sure there's only one EnemyManager
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
        enemyTemplate = enemyPrefab.GetComponent<Enemy>();
    }

    public static void CreateNewEnemy(MazeCell cell)
    {
        Vector2 spawnPosition = cell.GetWorldPosition();
        Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f));
        Enemy enemy = Instantiate(enemyTemplate, spawnPosition, rotation);
        enemy.AssignID(nextEnemyID);
        enemy.lastCellEntered = cell;
        enemies.Add(nextEnemyID, enemy);
        Debug.Log($"Create enemy object with ID of {nextEnemyID}");
        nextEnemyID++;
    }

    public static void CreateNewEnemy(Vector2 pos)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f));
        Enemy enemy = Instantiate(enemyTemplate, pos, rotation);
        enemy.AssignID(nextEnemyID);
        enemies.Add(nextEnemyID, enemy);
        Debug.Log($"Create enemy object with ID of {nextEnemyID}");
        nextEnemyID++;
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
            SendEnemyStates();
        }
    }

    public void SendEnemyStates()
    {
        string response = "EnemyManager:UpdateEnemyStates:";
        foreach (KeyValuePair<int, Enemy> e in enemies)
        {
            Vector2 nextCommand = e.Value.GetNextCommand();
            Vector2 curVelocity = e.Value.GetVelocity();
            response += e.Key + "|" + e.Value.GetXPosition() + "|" + e.Value.GetYPosition() + "|" + e.Value.isAngry +
            "|" + nextCommand.x + "|" + nextCommand.y + "|" + curVelocity.x + "|" + curVelocity.y +  "/";
        }

        UDPHost.instance.SendDataToClients(response);
    }

    public static void UpdateEnemyState(int id, Vector2 pos, bool isAngry, Vector2 nextCommand, Vector2 curVelocity)
    {
        if (!enemies.ContainsKey(id))
        {
            Debug.Log("Adding enemy " + id);
            CreateNewEnemy(pos);
        }

        enemies[id].SetPosition(pos);
        enemies[id].isAngry = isAngry;
        if (enemies[id].GetNextCommand() != nextCommand)
        {
            enemies[id].SetNextCommand(nextCommand);
        }

        if (enemies[id].GetVelocity() != curVelocity)
        {
            enemies[id].SetVelocity(curVelocity);
        }
    }

    public static void RestartEnemyMoveJourney(int id)
    {
        enemies[id].MoveToNewPositionInMaze();
    }

    public static void UpdateEnemyStates(string message)
    {
        if (!SystemManager.instance.isInGame()) return;

        string[] enemyData = message.Split('/');
        Vector2 nextCommand, curVelocity;
        for (int i = 0; i < enemyData.Length - 1; i++)
        {
            string[] currentEnemyData = enemyData[i].Split('|');
            int id = int.Parse(currentEnemyData[0]);
            Vector2 pos = new Vector2(float.Parse(currentEnemyData[1]), float.Parse(currentEnemyData[2]));
            bool isAngry = bool.Parse(currentEnemyData[3]);
            nextCommand = new Vector2(float.Parse(currentEnemyData[4]), float.Parse(currentEnemyData[5]));
            curVelocity = new Vector2(float.Parse(currentEnemyData[6]), float.Parse(currentEnemyData[7]));
            UpdateEnemyState(id, pos, isAngry, nextCommand, curVelocity);
        }
    }

    public static void HideEnemy(int enemyID)
    {
        enemies[enemyID].HideEnemy();
    }

    public void CreateEnemies(int numberOfEnemies)
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            // Create enemy
            CreateNewEnemy(MazeManager.instance.GetRandomMazeCell());
        }
    }

    public void Reset()
    {
        CancelInvoke();

        foreach (KeyValuePair<int, Enemy> enemy in enemies)
        {
            enemy.Value.Delete();
        }

        enemies.Clear();
        nextEnemyID = 0;
    }
}

