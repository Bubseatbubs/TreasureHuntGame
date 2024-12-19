using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    [SerializeField]
    Player player;

    [SerializeField]
    AudioSource source;

    [SerializeField]
    AudioClip[] footstepNoise;

    float soundInterval = 0;
    const float MAX_SOUND_INTERVAL = 150;
    
    void FixedUpdate()
    {
        if (player.isMoving) {
            soundInterval += player.moveSpeed;
            if (soundInterval >= MAX_SOUND_INTERVAL)
            {
                source.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                source.clip = footstepNoise[UnityEngine.Random.Range(0, footstepNoise.Length)];
                source.Play();
                soundInterval = 0;
            }
        }

        if (!player.isMoving) {
            soundInterval = MAX_SOUND_INTERVAL * 0.9f;
        }
    }
}
