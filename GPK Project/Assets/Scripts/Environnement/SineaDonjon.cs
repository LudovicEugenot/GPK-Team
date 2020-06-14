using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineaDonjon : MonoBehaviour
{
    public WorldManager.EventName eventToDisappear;

    private WorldManager.WorldEvent worldEventToDisappear;

    void Start()
    {
        worldEventToDisappear = WorldManager.GetWorldEvent(eventToDisappear);
    }


    void Update()
    {
        if(worldEventToDisappear.occured)
        {
            Destroy(gameObject);
        }
    }
}
