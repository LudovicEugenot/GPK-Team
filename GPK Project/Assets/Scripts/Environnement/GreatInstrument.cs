using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreatInstrument : MonoBehaviour
{
    public Hook hookToInteract;
    public WorldManager.EventName triggeredEvent;

    [HideInInspector] public bool isRelived;
    private WorldManager.WorldEvent triggeredWorldEvent;
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        triggeredWorldEvent = WorldManager.GetWorldEvent(triggeredEvent);
        if(triggeredWorldEvent.occured)
        {
            isRelived = true;
            animator.SetBool("Relived", true);
        }
    }


    void Update()
    {
        if(Input.GetButtonDown("Blink") && !GameManager.Instance.blink.IsSelecting() && (Vector2)GameManager.Instance.player.transform.position == (Vector2)hookToInteract.transform.position)
        {
            isRelived = true;
            triggeredWorldEvent.occured = true;
            animator.SetBool("Relive", true);
        }
    }
}
