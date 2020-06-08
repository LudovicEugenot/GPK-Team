using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheBroken : MonoBehaviour
{
    public WorldManager.EventName eventToDisappear;

    private WorldManager.WorldEvent worldEventToDisappear;

    private Animator animator;
    void Start()
    {
        worldEventToDisappear = WorldManager.GetWorldEvent(eventToDisappear);
        animator = GetComponent<Animator>();
        if(worldEventToDisappear.occured)
        {
            animator.SetInteger("Disappear", 2);
        }
    }

    void Update()
    {
        if (worldEventToDisappear.occured)
        {
            animator.SetInteger("Disappear", 1);
        }
    }
}
