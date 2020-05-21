using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicHook : Hook
{
    [Header("Classic Hook Options")]
    public bool convertable;
    public Color convertedColor;
    public float effectRange;
    public GameObject effectCircleVisualO;

    [HideInInspector] public bool converted;
    private Vector2 initialCircleScale;

    void Start()
    {
        HandlerStart();
        converted = false;
        //initialCircleScale = effectCircleVisualO.transform.localScale;
    }

    void Update()
    {
        HandlerUpdate();
    }

    public override void StateUpdate()
    {
        if((Vector2.Distance(GameManager.Instance.blink.transform.position, transform.position) <= GameManager.Instance.blink.currentRange || converted))
        {
            selectable = true;
            blinkable = PlayerInSight();
        }
        else
        {
            selectable = false;
            blinkable = false;
        }

        sprite.color = blinkable ? (selected ? selectedColor : (converted ? convertedColor : blinkableColor)) : unBlinkableColor;
    }

    public override IEnumerator BlinkSpecificReaction()
    {
        converted = convertable ? true : false;
        yield return null;
    }
}
