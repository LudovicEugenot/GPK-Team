using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicHook : Hook
{
    [Header("Classic Hook Options")]
    public bool convertable;
    public Color convertedColor;

    [HideInInspector] public bool converted;

    void Start()
    {
        HandlerStart();
        converted = false;
    }

    void Update()
    {
        HandlerUpdate();
    }

    public override void StateUpdate()
    {
        if(Vector2.Distance(blink.transform.position, transform.position) <= blink.currentRange || converted)
        {
            blinkable = true;
        }
        else
        {
            blinkable = false;
        }

        sprite.color = blinkable ? (selected ? selectedColor : (converted ? convertedColor : blinkableColor)) : unselectableColor;
    }

    public override IEnumerator BlinkReaction()
    {
        converted = convertable ? true : false;
        yield return null; // conversion animation effect
    }
}
