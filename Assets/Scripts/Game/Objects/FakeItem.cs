using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class FakeItem : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public Player playerToGoTo;
    public Vector2 positionToGoTo { get; private set; }
    const float speed = 12f;

    [SerializeField]
    AudioClip pickupNoise;

    void Start()
    {
        AudioSource.PlayClipAtPoint(pickupNoise, this.transform.position);
    }

    public void SetPlayerToGoTo(Player player)
    {
        playerToGoTo = player;
    }

    void FixedUpdate()
    {
        Vector2 playerVelocity = playerToGoTo.rb2d.velocity / playerToGoTo.moveSpeed;
        positionToGoTo = playerToGoTo.rb2d.position;
        rb2d.position = Vector2.Lerp(rb2d.position, positionToGoTo, Time.deltaTime * speed);
        transform.position = rb2d.position;
        if (Math.Abs(rb2d.position.x - positionToGoTo.x) < 0.4f &&
        Math.Abs(rb2d.position.y - positionToGoTo.y) < 0.4f) {
            Destroy(gameObject);
        }
    }
}
