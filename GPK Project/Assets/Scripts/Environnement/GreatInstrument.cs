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
        ZoneHandler.Instance.reliveRemotlyChanged = true;
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
        if(isRelived)
        {
            ZoneHandler.Instance.currentReliveProgression = 1;
        }
        else
        {
            ZoneHandler.Instance.currentReliveProgression = 0;
            ZoneHandler.Instance.reliveRemotlyChanged = true;
        }

        if(Input.GetButtonDown("Blink") && PlayerManager.CanInteract() && (Vector2)GameManager.Instance.player.transform.position == (Vector2)hookToInteract.transform.position && !isRelived)
        {
            isRelived = true;
            triggeredWorldEvent.occured = true;
            animator.SetBool("Relive", true);
            GameManager.playerAnimator.SetTrigger("Throw");
        }
    }
}
