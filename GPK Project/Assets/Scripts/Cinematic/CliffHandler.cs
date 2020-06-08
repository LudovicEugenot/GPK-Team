using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CliffHandler : MonoBehaviour
{
    public WorldManager.EventName eventToFall;

    private WorldManager.WorldEvent worldEventToFall;
    private bool fell;
    private Animator animator;

    void Start()
    {
        worldEventToFall = WorldManager.GetWorldEvent(eventToFall);
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateCliffState();
    }

    private void UpdateCliffState()
    {
        if(worldEventToFall.occured && !fell)
        {
            fell = true;
            animator.SetTrigger("Fall");
        }
    }
}
