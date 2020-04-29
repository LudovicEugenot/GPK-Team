using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookterruptor : Hook
{
    public Color pressedColor;
    public bool stayPressedUntilNextBlink;
    public float pressMinTime;

    public WorldManager.EventName eventToOccur;
    private WorldManager.WorldEvent worldEventToOccur;

    [HideInInspector] public bool pressed;

    void Start()
    {
        HandlerStart();
        pressed = false;

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
        if(stayPressedUntilNextBlink)
        {
            pressed = true;
        }
        else
        {
            pressed = true;
            yield return new WaitForSeconds(pressMinTime);
            pressed = false;
        }
    }

    public override void StateUpdate()
    {
        if (stayPressedUntilNextBlink)
        {
            if(pressed && GameManager.Instance.blink.currentHook != this)
            {
                Invoke("Unpress", pressMinTime);
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

    private void Unpress()
    {
        pressed = false;
    }
}
