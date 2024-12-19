using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.IO.LowLevel.Unsafe;
using System;
using TMPro;
using System.Linq;
using Unity.VisualScripting;

public class Player : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public float baseMoveSpeed;
    public float moveSpeed { get; private set; }
    public Rigidbody2D rb2d;
    public Vector2 forceToApply;
    public float forceDamping;
    public String username { get; private set; }
    public int ID { get; private set; }
    public double weight { get; private set; }
    public float speedMultiplier { get; private set; }
    public int inventoryCount { get; private set; }
    private Vector2 PlayerInput;
    private PriorityQueue<Item, int> inventory = new PriorityQueue<Item, int>();
    public double realCarriedValue { get; private set; }
    public double carriedValue { get; private set; }
    public double carriedValueMultiplier { get; private set; }
    public double balance { get; private set; }
    private PlayerStats statWindow;
    float rotationSpeed = 15f;

    [SerializeField]
    Animator animator;

    private static readonly Quaternion LeftRotation = Quaternion.Euler(0f, 180f, 0f);
    private static readonly Quaternion RightRotation = Quaternion.Euler(0f, 0f, 0f);
    bool facingRight;
    public bool isMoving { get; private set; }

    [SerializeField]
    private TextMeshProUGUI usernameDisplay;

    [SerializeField]
    private Canvas usernameCanvas;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkController.ID == ID)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            virtualCamera.Follow = rb2d.transform;
            statWindow = FindObjectOfType<PlayerStats>();
        }

        weight = 0;
        inventoryCount = 0;
        carriedValue = 0;
        facingRight = true;
        usernameCanvas.transform.SetParent(null);
        usernameDisplay.transform.rotation = Quaternion.identity;
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (NetworkController.ID == ID)
        {
            PlayerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        }

        // Rotate player if direction changed
        if (facingRight && transform.rotation != RightRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, RightRotation, Time.deltaTime * rotationSpeed);
        }
        else if (!facingRight && transform.rotation != LeftRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, LeftRotation, Time.deltaTime * rotationSpeed);
        }

        Move(PlayerInput);
    }

    void Update()
    {
        if (NetworkController.ID == ID)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Pickup Item logic
                if (NetworkController.isHost)
                {
                    PlayerManager.CheckForItemPickup(ID);
                }
                else
                {
                    RequestItemPickup();
                }
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                // Pickup Item logic
                if (NetworkController.isHost)
                {
                    DropItem();
                    TCPHost.instance.SendDataToClients($"PlayerManager:DropPlayerItem:{ID}");
                }
                else
                {
                    RequestItemDrop();
                }
            }
        }
    }

    void LateUpdate()
    {
        // Keep the text above the player but free from rotation
        usernameCanvas.transform.position = transform.position + new Vector3(0, 0.6f, 0);
    }

    void Move(Vector2 input)
    {
        moveSpeed = baseMoveSpeed * (1 - speedMultiplier);

        Vector2 moveForce = input * moveSpeed;
        ApplyForce(moveForce);

        // Animation
        if (Math.Abs(moveForce.x) + Math.Abs(moveForce.y) == 0)
        {
            animator.SetFloat("Speed", 0);
        }
        else {
            float moveSpeedPercentage = moveSpeed / baseMoveSpeed;
            animator.SetFloat("Speed", moveSpeedPercentage);
        }

        // Set bool for if player is moving
        if (moveForce.x != 0 || moveForce.y != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        // Check direction of player
        if (moveForce.x < 0)
        {
            // Turn left
            facingRight = false;
        }
        else if (moveForce.x > 0)
        {
            // Turn right
            facingRight = true;
        }
    }

    void ApplyForce(Vector2 moveForce)
    {
        moveForce += forceToApply;
        forceToApply /= forceDamping;

        if (Mathf.Abs(forceToApply.x) <= 0.01f && Mathf.Abs(forceToApply.y) <= 0.01f)
        {
            forceToApply = Vector2.zero;
        }

        rb2d.velocity = moveForce;
    }

    public void AssignID(int id)
    {
        ID = id;
    }

    public void AssignUsername(String name)
    {
        username = name;
        if (NetworkController.ID != ID)
        {
            usernameDisplay.text = username;
        }
    }

    public float GetXPosition()
    {
        return rb2d.position.x;
    }

    public float GetYPosition()
    {
        return rb2d.position.y;
    }

    public Vector2 GetPosition()
    {
        return rb2d.position;
    }

    public void SetPosition(Vector2 pos)
    {
        rb2d.position = pos;
    }

    public Vector2 GetInput()
    {
        return PlayerInput;
    }

    public void SetInput(Vector2 Input)
    {
        PlayerInput = Input;
    }

    // Item logic

    void RequestItemPickup()
    {
        TCPConnection.instance.SendDataToHost($"PlayerManager:CheckForItemPickup:{ID}");
    }

    public void AddItem(int itemID)
    {
        Item item;
        if (!ItemManager.items.TryGetValue(itemID, out item))
        {
            Debug.Log($"Item {itemID} doesn't exist!");
        }

        Debug.Log($"{username} picked up item {itemID}");

        // Update player info
        inventory.Enqueue(item, itemID);
        realCarriedValue += item.value;
        carriedValueMultiplier += 0.15f;
        carriedValue = Math.Round(realCarriedValue * (1 + carriedValueMultiplier), 2);
        weight += item.weight;
        speedMultiplier = Math.Clamp(0.01f * ((float)weight / 5), 0, 0.95f);
        inventoryCount++;

        // Update UI
        if (NetworkController.ID == ID)
        {
            statWindow.UpdateStats(weight, carriedValue, inventoryCount, balance);
        }
    }

    void RequestItemDrop()
    {
        TCPConnection.instance.SendDataToHost($"PlayerManager:DropPlayerItem:{ID}");
    }

    public void DropItem()
    {
        if (inventory.Count <= 0)
        {
            Debug.Log($"{username} doesn't have any items to drop!");
            return;
        }

        Item item = inventory.Dequeue();
        item.RespawnItem();
        item.SetPosition(rb2d.position);
        Debug.Log($"{username} dropped item {item.ID}");

        // Update player info
        realCarriedValue -= item.value;
        carriedValueMultiplier -= 0.15f;
        carriedValue = Math.Round(realCarriedValue * (1 + carriedValueMultiplier), 2);
        weight -= item.weight;

        if (weight < 0.01)
        {
            weight = 0;
        }

        speedMultiplier = Math.Clamp(0.01f * ((float)weight / 5), 0, 0.95f);
        inventoryCount--;

        // Update UI
        if (NetworkController.ID == ID)
        {
            statWindow.UpdateStats(weight, carriedValue, inventoryCount, balance);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (NetworkController.isHost)
        {
            if (collision.gameObject.CompareTag("SpawnArea") && inventoryCount > 0)
            {
                SendItemReturnToClients();
                PlayerManager.ReturnPlayerItems(ID);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (NetworkController.isHost)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Kill();
                TCPHost.instance.SendDataToClients($"PlayerManager:KillPlayer:{ID}");
            }
        }
    }

    private void SendItemReturnToClients()
    {
        // Client asks host if they can pick up an item
        TCPHost.instance.SendDataToClients($"PlayerManager:ReturnPlayerItems:{ID}");
    }

    public void ReturnItems()
    {
        // Update player info
        balance += carriedValue;
        int beforeInventoryCount = inventoryCount;
        ResetPlayerStats();
        inventory.Clear();

        string itemOrItems = (beforeInventoryCount == 1) ? "item" : "items";

        if (NetworkController.isHost)
        {
            ChatManager.instance.SendSystemMessage($"{username} cashed in {beforeInventoryCount} {itemOrItems} and now has a balance of ${balance}!");
        }


        // Update UI
        if (NetworkController.ID == ID)
        {
            statWindow.UpdateStats(weight, carriedValue, inventoryCount, balance);
        }
    }

    public void Kill()
    {
        // Drop everything in inventory
        while (inventory.Count > 0)
        {
            DropItem();
        }

        inventory.Clear();
        transform.position = Vector2.zero;
        rb2d.position = Vector2.zero;

        ResetPlayerStats();
        if (NetworkController.ID == ID)
        {
            statWindow.UpdateStats(weight, carriedValue, inventoryCount, balance);
        }
    }

    private void ResetPlayerStats()
    {
        carriedValue = 0.0;
        realCarriedValue = 0.0;
        weight = 0.0;
        carriedValueMultiplier = 0.0;
        speedMultiplier = 0f;
        inventoryCount = 0;
    }

    public void Delete()
    {
        Destroy(usernameCanvas);
        if (NetworkController.ID == ID)
        {
            statWindow.ResetStats();
        }

        inventory.Clear();
        ResetPlayerStats();
        Destroy(gameObject);
    }
}
