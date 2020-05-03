using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int maxHealthPoint;
    public int health; //half an health point
    public float[] position;
    public bool ownSpeaker;

    public PlayerData(PlayerManager player)
    {
        maxHealthPoint = player.maxhealthPoint;
        health = player.currentHealth;
        position = new float[2];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        ownSpeaker = player.ownSpeaker;
    }
}
