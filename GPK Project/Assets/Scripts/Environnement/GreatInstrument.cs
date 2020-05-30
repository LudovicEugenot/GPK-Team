using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreatInstrument : MonoBehaviour
{
    public Hook hookToInteract;
    public Talk triggeredTalk;
    public float timeBeforeTalk;
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

        if(GameManager.Instance.blink.currentHook == hookToInteract && !isRelived && PlayerManager.CanInteract())
        {
            PlayerManager.DisplayIndicator();

            if (Input.GetButtonDown("Blink") && PlayerManager.IsMouseNearPlayer())
            {
                isRelived = true;
                triggeredWorldEvent.occured = true;
                animator.SetBool("Relive", true);
                GameManager.playerAnimator.SetTrigger("Throw");
                StartCoroutine(StartTalk());
            }
        }
    }

    private IEnumerator StartTalk()
    {
        yield return new WaitForSeconds(timeBeforeTalk);
        if(triggeredTalk != null)
        {
            GameManager.Instance.dialogueManager.StartTalk(triggeredTalk, transform, 4);
        }
    }
}
