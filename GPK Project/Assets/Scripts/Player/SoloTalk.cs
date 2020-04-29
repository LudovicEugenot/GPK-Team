using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoloTalk : MonoBehaviour
{
    public Talk commentary;
    public Hook nearbyHook;
    public float camZoom;
    public WorldManager.StoryStep storyStepRequired;
    public WorldManager.EventName[] requiredEvents;


    private WorldManager.WorldEvent[] requiredWorldEvents;

    void Start()
    {
        SetupWE();
    }

    void Update()
    {
        if(Input.GetButtonDown("Interact") && GameManager.Instance.blink.currentHook == nearbyHook && storyStepRequired <= WorldManager.currentStoryStep && IsValid())
        {
            GameManager.Instance.dialogueManager.StartTalk(commentary, transform, camZoom);
        }
    }

    private void SetupWE()
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
