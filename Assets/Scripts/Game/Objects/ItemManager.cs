using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField]
    private GameObject itemPrefab;
    private static Item itemTemplate;
    public static Dictionary<int, Item> items = new Dictionary<int, Item>();
    public static ItemManager instance;
    private static int nextItemID = 0;

    /*
    Singleton Pattern: Make sure there's only one ItemManager
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
        itemTemplate = itemPrefab.GetComponent<Item>();
    }

    public static void CreateNewItem(Vector2 spawnPosition)
    {
        Item item = Instantiate(itemTemplate, spawnPosition, Quaternion.identity);
        item.AssignID(nextItemID);
        items.Add(nextItemID, item);
        Debug.Log($"Create item object with ID of {nextItemID}");
        nextItemID++;
    }

    public static void UpdateItemState(int id, Vector2 pos, bool isPickedUp)
    {
        if (!items.ContainsKey(id))
        {
            Debug.Log("Adding item " + id);
            CreateNewItem(pos);
        }

        items[id].SetPosition(pos);
        if (items[id].pickedUp && isPickedUp == false)
        {
            Debug.Log($"Hid item {id}");
            items[id].HideItem();
        }
        else if (!items[id].pickedUp && isPickedUp == true)
        {
            Debug.Log($"Showed item {id}");
            items[id].RespawnItem();
        }
    }

    public static void UpdateItemStates(string message)
    {
        string[] itemData = message.Split('/');
        for (int i = 0; i < itemData.Length - 1; i++)
        {
            string[] currentItemData = itemData[i].Split('|');
            int id = int.Parse(currentItemData[0]);
            Vector2 pos = new Vector2(float.Parse(currentItemData[1]), float.Parse(currentItemData[2]));
            bool isPickedUp = bool.Parse(currentItemData[3]);
            UpdateItemState(id, pos, isPickedUp);
        }
    }

    public static void HideItem(int itemID)
    {
        items[itemID].HideItem();
    }

    public static void StartItemPickupAnimation(int itemID, Player player)
    {
        items[itemID].StartItemPickupAnimation(player);
    }

    public static void RespawnItem(int itemID)
    {
        items[itemID].RespawnItem();
    }

    public void CreateItems(int numberOfItems)
    {
        for (int i = 0; i < numberOfItems; i++)
        {
            // Create item
            CreateNewItem(MazeManager.instance.GetRandomSpawnPosition());
        }
    }

    public void Reset()
    {
        CancelInvoke();

        foreach (KeyValuePair<int, Item> item in items)
        {
            item.Value.Delete();
        }

        items.Clear();
        nextItemID = 0;
    }
}
