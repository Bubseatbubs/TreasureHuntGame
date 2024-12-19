using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class Item : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public int value { get; private set; }
    public double weight { get; private set; }
    public int ID { get; private set; }
    public bool pickedUp { get; private set; }

    [SerializeField]
    private GameObject fakeItemTemplate;

    // Start is called before the first frame update
    void Start()
    {
        // Add random attributes to the item
        value = UnityEngine.Random.Range(10, 50);
        weight = Math.Round(value * UnityEngine.Random.Range(0.9f, 1.1f), 2);
        this.transform.localScale = new Vector3((float) weight / 5f, (float) weight / 5f, 1);
    }

    public void AssignID(int id)
    {
        ID = id;
    }

    public void SetPosition(Vector2 pos)
    {
        this.transform.position = pos;
        rb2d.position = pos;
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

    public void StartItemPickupAnimation(Player player)
    {
        GameObject fakeItem = Instantiate(fakeItemTemplate, rb2d.position, Quaternion.identity);
        FakeItem fakeItemClass = fakeItem.GetComponent<FakeItem>();
        fakeItem.transform.localScale = this.transform.localScale;
        fakeItemClass.SetPlayerToGoTo(player);
    }

    public void HideItem()
    {
        pickedUp = true;
        gameObject.SetActive(false);
    }

    public void RespawnItem()
    {
        pickedUp = false;
        gameObject.SetActive(true);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log($"Item {ID} is touching a wall");
            rb2d.position += new Vector2(rb2d.position.x + 0.25f, rb2d.position.y);
        }
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}
