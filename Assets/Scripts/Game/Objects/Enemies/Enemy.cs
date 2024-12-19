using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Rigidbody2D rb2d;

    [SerializeField]
    Stack<Vector2> moveCommands = new Stack<Vector2>();
    public bool isAngry = false;
    public int ID { get; private set; }

    public float speed = 4.0f;
    float scale = 1f;
    public int checkTimerInterval = 10;
    int interval = 0;

    [SerializeField]
    private GameObject enemyObject;

    [SerializeField]
    private LayerMask targetLayers;

    public MazeCell lastCellEntered;
    float rotationSpeed = 15f;

    private static readonly Quaternion LeftRotation = Quaternion.Euler(0f, 180f, 0f);
    private static readonly Quaternion RightRotation = Quaternion.Euler(0f, 0f, 0f);
    bool facingRight;
    float lastPosition;

    void Start()
    {
        speed = UnityEngine.Random.Range(3f, 4f);
        scale = UnityEngine.Random.Range(2f, 4f);
        this.transform.localScale = new Vector3(scale, scale, 1);
        facingRight = true;

        MoveToNewPositionInMaze();
    }

    public void MoveToNewPositionInMaze()
    {
        rb2d.velocity = new Vector2(0, 0);
        Debug.DrawLine(transform.position, (Vector2)transform.position + new Vector2(1, 1), Color.green, 5f);
        MazeCell start = lastCellEntered;
        bool foundValidCell = false;
        MazeCell destination = start;
        while (!foundValidCell)
        {
            destination = MazeManager.instance.GetRandomMazeCell();
            if (destination.connections.Count > 0)
            {
                foundValidCell = true;
            }
        }

        moveCommands = Pathfinding.AStarSearch(lastCellEntered, destination);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lastPosition = rb2d.position.x;
        if (!isAngry)
        {
            RoamingState();
        }
        else
        {
            ChasingState();
        }

        if (NetworkController.isHost)
        {
            interval++;
            if (interval < checkTimerInterval) return;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 6f, targetLayers);
            if (NetworkController.isHost && colliders.Length > 0)
            {
                isAngry = true;
                EnemyManager.instance.SendEnemyStates();
            }
        }
    }

    void LateUpdate()
    {
        RotateEnemy(lastPosition);
    }

    private void RotateEnemy(float lastPosition)
    {
        float offsetPosition = rb2d.position.x - lastPosition;

        if (offsetPosition < 0)
        {
            // Turn left
            facingRight = false;
        }
        else if (offsetPosition > 0)
        {
            // Turn right
            facingRight = true;
        }

        if (facingRight && transform.rotation != RightRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, RightRotation, Time.deltaTime * rotationSpeed);
        }
        else if (!facingRight && transform.rotation != LeftRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, LeftRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void RoamingState()
    {
        float step = speed * Time.deltaTime;

        if (moveCommands == null || moveCommands.Count <= 0)
        {
            MoveToNewPositionInMaze();
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, moveCommands.Peek(), step);
        rb2d.position = Vector2.MoveTowards(rb2d.position, moveCommands.Peek(), step);

        if (Mathf.Approximately(transform.position.x, moveCommands.Peek().x) &&
        Mathf.Approximately(transform.position.y, moveCommands.Peek().y))
        {
            moveCommands.Pop();
        }
    }

    void ChasingState()
    {
        float step = speed * 2 * Time.deltaTime;
        Vector2 closestPlayerPosition;
        try
        {
            closestPlayerPosition = GetClosestPlayer(12f).position;
        }
        catch (NullReferenceException)
        {
            closestPlayerPosition = transform.position;
        }

        Vector2 direction = (closestPlayerPosition - (Vector2)transform.position).normalized;
        rb2d.velocity = direction * speed * 1.5f;
    }

    Transform GetClosestPlayer(float radius)
    {
        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, radius, targetLayers);
        if (NetworkController.isHost && players.Length == 0)
        {
            isAngry = false;
            TCPHost.instance.SendDataToClients($"EnemyManager:RestartEnemyMoveJourney:{ID}");
            MoveToNewPositionInMaze();
            EnemyManager.instance.SendEnemyStates();
            return transform;
        }

        Transform closestPlayerTransform = null;
        float minimumDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Collider2D t in players)
        {
            float distance = Vector2.Distance(t.transform.position, currentPos);
            if (distance < minimumDistance)
            {
                closestPlayerTransform = t.transform;
                minimumDistance = distance;
            }
        }
        return closestPlayerTransform;
    }


    public float GetXPosition()
    {
        return rb2d.position.x;
    }

    public float GetYPosition()
    {
        return rb2d.position.y;
    }

    public void AssignID(int id)
    {
        ID = id;
    }

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
        rb2d.position = pos;
    }

    public Vector2 GetNextCommand()
    {
        if (moveCommands.Count > 1)
        {
            return moveCommands.Peek();
        }
        else
        {
            return transform.position;
        }
    }

    public void SetNextCommand(Vector2 newPos)
    {
        if (moveCommands.Count > 0)
        {
            moveCommands.Pop();
        }
        moveCommands.Push(newPos);
    }

    public Vector2 GetVelocity()
    {
        return rb2d.velocity;
    }

    public void SetVelocity(Vector2 curVelocity)
    {
        rb2d.velocity = curVelocity;
    }

    public void HideEnemy()
    {
        isAngry = false;
        enemyObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cell"))
        {
            lastCellEntered = collision.gameObject.GetComponent<MazeCell>();
        }
        else if (isAngry && collision.gameObject.CompareTag("SpawnArea") && NetworkController.isHost)
        {
            interval = -checkTimerInterval * 50;
            isAngry = false;
            TCPHost.instance.SendDataToClients($"EnemyManager:RestartEnemyMoveJourney:{ID}");
            MoveToNewPositionInMaze();
            EnemyManager.instance.SendEnemyStates();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (NetworkController.isHost)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                interval = -checkTimerInterval * 50;
                isAngry = false;
                TCPHost.instance.SendDataToClients($"EnemyManager:RestartEnemyMoveJourney:{ID}");
                MoveToNewPositionInMaze();
                EnemyManager.instance.SendEnemyStates();
            }
        }
    }

    public void Delete()
    {
        moveCommands.Clear();
        lastCellEntered = null;
        Destroy(gameObject);
    }
}
