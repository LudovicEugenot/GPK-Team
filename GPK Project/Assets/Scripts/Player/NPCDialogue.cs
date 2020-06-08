using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogue : MonoBehaviour
{
    public Hook hookToTalk;
    public Transform cinematicLookPos;
    [Range(0.0f, 10.0f)] public float cinematicLookZoom;
    public Dialogue[] dialogues;

    private Dialogue currentDialogue;
    private bool inDialogue;

    void Start()
    {
        foreach(Dialogue dialogue in dialogues)
        {
            dialogue.SetupWorldEvents();
        }
    }

    void Update()
    {
        TestDialogueStart();
    }

    void TestDialogueStart()
    {
        if(GameManager.Instance.blink.currentHook == hookToTalk && !GameManager.Instance.paused && !GameManager.Instance.dialogueManager.isTalking && PlayerManager.CanInteract())
        {
            PlayerManager.DisplayIndicator();

            if (Input.GetButtonDown("Blink") && PlayerManager.IsMouseNearPlayer())
            {
                currentDialogue = GetValidDialogue();
                if (currentDialogue != null)
                {
                    GameManager.Instance.dialogueManager.StartTalk(currentDialogue.talk, cinematicLookPos.position, cinematicLookZoom);
                    inDialogue = true;
                }
            }
        }

        if(inDialogue && !GameManager.Instance.dialogueManager.isTalking)
        {
            inDialogue = false;
            WorldManager.GetWorldEvent(currentDialogue.triggeredEvent).occured = true;
        }
    }


    private Dialogue GetValidDialogue()
    {
        List<Dialogue> validDialogues = new List<Dialogue>();
        foreach(Dialogue dialogue in dialogues)
        {
            if (dialogue.IsValid())
            {
                validDialogues.Add(dialogue);
            }
        }

        if(validDialogues.Count > 0)
        {
            return validDialogues[Random.Range(0, validDialogues.Count)];
        }
        else
        {
            return null;
        }
    }

    [System.Serializable]
    public class Dialogue
    {
        public Talk talk;
        public WorldManager.EventName triggeredEvent;
        public WorldManager.StoryStep progressionNeeded;
        public WorldManager.StoryStep lastValidStoryStep = WorldManager.StoryStep.EndGame;
        public List<WorldManager.EventName> requiredEvents;
        [HideInInspector] WorldManager.WorldEvent[] requiredWorldEvents;
        public List<WorldManager.EventName> compromisingEvents;
        [HideInInspector] WorldManager.WorldEvent[] compromisingWorldEvents;

        public void SetupWorldEvents()
        {
            requiredWorldEvents = new WorldManager.WorldEvent[requiredEvents.Count];
            for(int i = 0; i < requiredEvents.Count; i++)
            {
                requiredWorldEvents[i] = WorldManager.GetWorldEvent(requiredEvents[i]);
            }

            compromisingWorldEvents = new WorldManager.WorldEvent[compromisingEvents.Count];
            for (int i = 0; i < compromisingEvents.Count; i++)
            {
                compromisingWorldEvents[i] = WorldManager.GetWorldEvent(compromisingEvents[i]);
            }
        }

        public bool IsValid()
        {
            if(WorldManager.currentStoryStep >= progressionNeeded && (WorldManager.currentStoryStep <= lastValidStoryStep || lastValidStoryStep == WorldManager.StoryStep.Tutorial))
            {
                foreach(WorldManager.WorldEvent worldEvent in requiredWorldEvents)
                {
                    if(!worldEvent.occured && worldEvent.name != WorldManager.EventName.NullEvent)
                    {
                        return false;
                    }
                }

                foreach (WorldManager.WorldEvent worldEvent in compromisingWorldEvents)
                {
                    if (worldEvent.occured && worldEvent.name != WorldManager.EventName.NullEvent)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
