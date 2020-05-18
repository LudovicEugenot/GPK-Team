using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoloTalk : MonoBehaviour
{
    public Talk commentary;
    public Hook nearbyHook;
    public bool autoTrigger;
    public bool manualTrigger;
    public bool isCommentary;
    public float timeBewteenSentence;
    public SoloTalk previousTalk;
    public float camZoom;
    public Vector2 alternateBoxPos;
    public WorldManager.StoryStep storyStepRequired;
    public WorldManager.EventName[] requiredEvents;
    public GameObject interactionIndicator;

    private ParticleSystem shinyParticle;
    private WorldManager.WorldEvent[] requiredWorldEvents;
    private bool waitingForPreviousTalk;
    [HideInInspector] public bool talkStarted;

    void Start()
    {
        SetupWorldEvents();
        waitingForPreviousTalk = true;
        shinyParticle = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        if (previousTalk == null)
        {
            if (GameManager.Instance.blink.currentHook == nearbyHook && storyStepRequired <= WorldManager.currentStoryStep && IsValid())
            {
                if(shinyParticle!= null && !shinyParticle.isPlaying)
                {
                    shinyParticle.Play();
                }

                if(interactionIndicator != null)
                {
                    interactionIndicator.SetActive(true);
                }

                if (((Input.GetButtonDown("Blink") && manualTrigger) && PlayerManager.CanInteract()) && !GameManager.Instance.dialogueManager.isTalking || autoTrigger)
                {
                    if (GameManager.Instance.usePlaytestRecord && interactionIndicator == null)
                    {
                        PlayTestRecorder.currentZoneRecord.loreHolderInteracted.Add(gameObject.name);
                    }

                    if (isCommentary)
                    {
                        GameManager.Instance.dialogueManager.StartCommentary(commentary, timeBewteenSentence, alternateBoxPos);
                    }
                    else
                    {
                        GameManager.Instance.dialogueManager.StartTalk(commentary, transform, camZoom);
                    }
                    talkStarted = true;
                    autoTrigger = false;

                }
            }
            else
            {
                if(shinyParticle != null && !shinyParticle.isStopped)
                {
                    shinyParticle.Stop();
                }

                if (interactionIndicator != null)
                {
                    interactionIndicator.SetActive(false);
                }
            }
        }
        else
        {
            if (previousTalk.talkStarted && waitingForPreviousTalk && !GameManager.Instance.dialogueManager.isTalking)
            {
                GameManager.Instance.dialogueManager.StartTalk(commentary, transform, camZoom);
                waitingForPreviousTalk = false;
            }
        }

        if(shinyParticle != null && !shinyParticle.isStopped && GameManager.Instance.dialogueManager.isTalking)
        {
            shinyParticle.Stop();
        }
    }

    private void SetupWorldEvents()
    {
        requiredWorldEvents = new WorldManager.WorldEvent[requiredEvents.Length];
        for(int i = 0; i < requiredEvents.Length; i++)
        {
            requiredWorldEvents[i] = WorldManager.GetWorldEvent(requiredEvents[i]);
        }
    }

    private bool IsValid()
    {
        foreach(WorldManager.WorldEvent worldEvent in requiredWorldEvents)
        {
            if(!worldEvent.occured)
            {
                return false;
            }
        }
        return true;
    }
}
