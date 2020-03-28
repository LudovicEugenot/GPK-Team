using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookterruptor : Hook
{
    public Color pressedColor;
    public bool stayPressedUntilNextBlink;
    public float pressMinTime;
    public SwitchElement[] connectedElements;

    private bool pressed;

    void Start()
    {
        HandlerStart();
        pressed = false;
    }

    void Update()
    {
        HandlerUpdate();
    }

    public override IEnumerator BlinkSpecificReaction()
    {
        foreach(SwitchElement element in connectedElements)
        {
            if(!stayPressedUntilNextBlink)
            {
                element.SwitchOnce();
            }
        }

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
            if(pressed && (Vector2)GameManager.Instance.blink.transform.position != (Vector2)transform.position)
            {
                pressed = false;
            }

            foreach (SwitchElement element in connectedElements)
            {
                if(pressed)
                {
                    element.SwitchOn();
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
