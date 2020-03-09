using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RythmHook : Hook
{
    [Header("Rythm Hook Options")]
    public bool[] lockLoop;
    public Color lockedColor;

    private int currentLoopProgression;
    private bool locked;

    public bool IsBlink;
    public Animator animator;

    void Start()
    {
        HandlerStart();
        currentLoopProgression = 0;
    }

    void Update()
    {
        HandlerUpdate();
        BlinkableUpdate();
    }

    private void BlinkableUpdate()
    {
        if(beatManager.onBeatSingleFrame)
        {
            Invoke("IncreaseProgression", beatManager.beatTime / 2);
        }
    }

    public override void StateUpdate()
    {
        locked = !lockLoop[currentLoopProgression];
        blinkable = !locked && Vector2.Distance(blink.transform.position, transform.position) <= blink.currentRange;

        sprite.color = !locked ? (blinkable ? (selected ? selectedColor : blinkableColor) : unselectableColor) : lockedColor;
    }

    public override IEnumerator BlinkReaction()
    {
        yield return null;
    }

    private void IncreaseProgression()
    {
        currentLoopProgression++;

        if (currentLoopProgression >= lockLoop.Length)
        {
            currentLoopProgression = 0;
        }
    }
}
