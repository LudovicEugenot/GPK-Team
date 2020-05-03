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

    public WorldManager.EventName eventToOccur;
    private WorldManager.WorldEvent worldEventToOccur;

    [HideInInspector] public bool pressed;
    private float pressTimeRemaining;

    void Start()
    {
        HandlerStart();
        pressed = false;
        pressTimeRemaining = 0;
        worldEventToOccur = WorldManager.GetWorldEvent(eventToOccur);
    }

    void Update()
    {
        HandlerUpdate();

        animator.SetBool("Pressed", pressed);

        if (eventToOccur != WorldManager.EventName.NullEvent)
        {
            if (pressed)
            {
                worldEventToOccur.occured = true;
            }
        }
    }

    public override IEnumerator BlinkSpecificReaction()
    {
        if(!switchInterruptor)
        {
            pressed = true;
            pressTimeRemaining = pressBeatTime * BeatManager.Instance.BeatTime + addedPressTime;
        }
        else
        {
            pressed = !pressed;
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
                    else
                    {
                        pressed = false;
                    }
                }
            }
            else
            {
                if (pressTimeRemaining > 0)
                {
                    pressTimeRemaining -= Time.deltaTime;
                }
                else
                {
                    pressed = false;
                }
            }
        }

        if (Vector2.Distance(GameManager.Instance.blink.transform.position, transform.position) <= GameManager.Instance.blink.currentRange)
        {
            blinkable = true;
        }
        else
        {
            blinkable = false;
        }

        sprite.color = !pressed ? (blinkable ? (selected ? selectedColor : blinkableColor) : unselectableColor) : pressedColor;
    }
}
