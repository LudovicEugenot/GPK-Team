using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreatInstrument : MonoBehaviour
{
    public Hook hookToInteract;
    public Talk triggeredTalk;
    public float timeBeforeTalk;
    public WorldManager.EventName triggeredEvent;
    public AudioClip reliveSound;

    [HideInInspector] public bool isRelived;
    private WorldManager.WorldEvent triggeredWorldEvent;
    private Animator animator;
    private AnimSynchronizer synchronizer;
    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
        synchronizer = GetComponent<AnimSynchronizer>();
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
            ZoneHandler.Instance.currentZone.isRelived = true;
        }
        else
        {
            ZoneHandler.Instance.currentReliveProgression = 0;
            ZoneHandler.Instance.currentZone.isRelived = false;
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
                source.PlayOneShot(reliveSound);
                StartCoroutine(StartTalk());
            }
        }
    }

    private IEnumerator StartTalk()
    {
        yield return new WaitForSeconds(timeBeforeTalk);
        if(triggeredTalk != null)
        {
            synchronizer.Synchronize();
            GameManager.Instance.dialogueManager.StartTalk(triggeredTalk, Vector2.zero, 5.5f);
        }
    }
}
