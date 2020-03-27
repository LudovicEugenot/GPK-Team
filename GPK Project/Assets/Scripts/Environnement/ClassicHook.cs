using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicHook : Hook
{
    [Header("Classic Hook Options")]
    public bool convertable;
    public Color convertedColor;
    public Animator animator;
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
            blinkable = true;
        }
        else
        {
            blinkable = false;
        }

        sprite.color = blinkable ? (selected ? selectedColor : (converted ? convertedColor : blinkableColor)) : unselectableColor;

        if(animator != null)
        {
            animator.SetBool("IsConvert", converted);
        }
    }

    public override IEnumerator BlinkReaction()
    {
        converted = convertable ? true : false;
        yield return null;
    }
}
