using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI weightText;

    [SerializeField]
    TextMeshProUGUI itemValueText;

    [SerializeField]
    TextMeshProUGUI balanceText;

    [SerializeField]
    Transform inventory;

    [SerializeField]
    GameObject itemPrefab;

    const float X_OFFSET = 20;
    const float Y_OFFSET = -100;

    void Start()
    {
        UpdateStats(0.0, 0, 0, 0);
    }

    public void UpdateStats(double weight, double itemValue, int inventoryCount, double balance)
    {
        weightText.text = $"{weight} LB";
        itemValueText.text = $"${itemValue}";
        balanceText.text = $"${balance}";
        UpdateInventoryCount(inventoryCount);
    }

    public void UpdateInventoryCount(int count)
    {
        // Clear all coins
        foreach (Transform item in inventory)
        {
            Destroy(item.gameObject);
        }

        // Put new coins
        for (int i = 0; i < count; i++)
        {
            GameObject coin = Instantiate(itemPrefab, inventory);
            Vector2 currentOffset = new Vector2(i * X_OFFSET, Y_OFFSET);
            coin.transform.position = (Vector2) coin.transform.position + currentOffset;
        }
    }

    public void ResetStats()
    {
        UpdateStats(0.0, 0, 0, 0);
        UpdateInventoryCount(0);
    }
}
