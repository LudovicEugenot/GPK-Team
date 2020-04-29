using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookterruptor : Hook
{
    public Color pressedColor;
    public bool stayPressedUntilNextBlink;
    public float pressBeatTime;

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
        pressed = true;
        pressTimeRemaining = pressBeatTime * BeatManager.Instance.BeatTime;
        yield return null;
    }

    public override void StateUpdate()
    {
        if(pressTimeRemaining > 0)
        {
            if (stayPressedUntilNextBlink)
            {
                if(GameManager.Instance.blink.currentHook != this)
                {
                    pressTimeRemaining -= Time.deltaTime;
                }
            }
            else
            {
                pressTimeRemaining -= Time.deltaTime;
            }
        }
        else
        {
            pressed = false;
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

    private void Unpress()
    {
        pressed = false;
    }
}
