using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleDoor : MonoBehaviour
{
    public WorldManager.EventName eventToTrigger;

    private WorldManager.WorldEvent worldEventToTrigger;
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        worldEventToTrigger = WorldManager.GetWorldEvent(eventToTrigger);
        if (worldEventToTrigger.occured)
        {
            animator.SetInteger("Open", 2);
        }
    }

    void Update()
    {
        if(worldEventToTrigger.occured)
        {
            animator.SetInteger("Open", 1);
        }
    }
}
