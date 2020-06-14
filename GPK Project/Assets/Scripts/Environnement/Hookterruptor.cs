using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookterruptor : Hook
{
    public Color pressedColor;
    public bool stayPressedUntilNextBlink;
    public float pressBeatTime;
    public float addedPressTime;
    public bool switchInterruptor;
    public float speakerTriggerDistance;
    public bool isActivator;
    [Header("Sounds")]
    public AudioClip pressSound;
    public AudioClip unpressSound;


    public WorldManager.EventName eventToOccur;
    private WorldManager.WorldEvent worldEventToOccur;

    [HideInInspector] public bool pressed;
    private float pressTimeRemaining;
    private bool speakerTriggerFlag;

    void Start()
    {
        HandlerStart();
        pressed = false;
        pressTimeRemaining = 0;
        worldEventToOccur = WorldManager.GetWorldEvent(eventToOccur);
        speakerTriggerFlag = true;
    }

    void Update()
    {
        HandlerUpdate();
        UpdateSpeakerProximity();
        animator.SetBool("Pressed", pressed);

        if (eventToOccur != WorldManager.EventName.NullEvent)
        {
            if (pressed)
            {
                worldEventToOccur.occured = true;
            }
        }
    }

    private void UpdateSpeakerProximity()
    {
        if (isActivator && Physics2D.OverlapCircle(transform.position, speakerTriggerDistance, LayerMask.GetMask("Speaker")) && GameManager.remoteSpeaker.speakerPlaced)
        {
            if (BeatManager.Instance.OnBeat(false, false, "(__)"))
            {
                hookState.relived = true;
            }

            if(!switchInterruptor)
            {
                if (stayPressedUntilNextBlink)
                {
                    pressed = true;
                    pressTimeRemaining = pressBeatTime * BeatManager.Instance.BeatTime + addedPressTime;
                    if (speakerTriggerFlag)
                    {
                        speakerTriggerFlag = false;
                        source.pitch = 1f;
                        source.PlayOneShot(pressSound);
                    }
                }
                else
                {
                    if (speakerTriggerFlag)
                    {
                        speakerTriggerFlag = false;
                        pressed = true;
                        pressTimeRemaining = pressBeatTime * BeatManager.Instance.BeatTime + addedPressTime;
                        source.pitch = 1f;
                        source.PlayOneShot(pressSound);
                    }
                }
            }
            else
            {
                if(speakerTriggerFlag)
                {
                    speakerTriggerFlag = false;
                    pressed = !pressed;
                    source.PlayOneShot(pressed ? pressSound : unpressSound);
                }
            }
        }
        else
        {
            speakerTriggerFlag = true;
        }
    }

    public override IEnumerator BlinkSpecificReaction()
    {
        if(!switchInterruptor)
        {
            pressed = true;
            pressTimeRemaining = pressBeatTime * BeatManager.Instance.BeatTime + addedPressTime;
            source.pitch = 1f;
            source.PlayOneShot(pressSound);
        }
        else
        {
            pressed = !pressed;
            source.PlayOneShot(pressed ? pressSound : unpressSound);
        }

        yield return null;
    }

    public override void StateUpdate()
    {
        if(!switchInterruptor)
        {
            if (stayPressedUntilNextBlink)
            {
                if (GameManager.Instance.blink.currentHook == this)
                {
                    pressed = true;
                    pressTimeRemaining = pressBeatTime * BeatManager.Instance.BeatTime + addedPressTime;
                }
                else
                {
                    if (pressTimeRemaining > 0)
                    {
                        pressTimeRemaining -= Time.deltaTime;
                    }
                    else if (pressed)
                    {
                        pressed = false;
                        source.pitch = 0.7f;
                        source.PlayOneShot(unpressSound);
                    }
                }
            }
            else
            {
                if (pressTimeRemaining > 0)
                {
                    pressTimeRemaining -= Time.deltaTime;
                }
                else if (pressed)
                {
                    pressed = false;
                    source.pitch = 0.7f;
                    source.PlayOneShot(unpressSound);
                }
            }
        }

        if (Vector2.Distance(GameManager.Instance.blink.transform.position, transform.position) <= GameManager.Instance.blink.currentRange && !isActivator)
        {
            selectable = true;
            blinkable = PlayerInSight();
        }
        else
        {
            selectable = false;
            blinkable = false;
        }

        sprite.color = isActivator ? blinkableColor : !pressed ? (blinkable ? (selected ? selectedColor : blinkableColor) : unBlinkableColor) : pressedColor;
    }
}
