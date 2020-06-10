using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MurVillage : MonoBehaviour
{
    public WorldManager.EventName eventToDestroy;
    private WorldManager.WorldEvent worldEventToDestroy;
    void Start()
    {
        worldEventToDestroy = WorldManager.GetWorldEvent(eventToDestroy);
    }

    void Update()
    {
        if(worldEventToDestroy.occured)
        {
            Destroy(gameObject);
        }
    }
}
