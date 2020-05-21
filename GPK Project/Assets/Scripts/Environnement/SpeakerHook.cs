using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerHook : Hook
{
    [HideInInspector] public bool isDisabled;
    [HideInInspector] public RemoteSpeaker remoteSpeaker;

    void Start()
    {
        HandlerStart();
    }

    void Update()
    {
        HandlerUpdate();
    }

    public override void StateUpdate()
    {
        if (!isDisabled)
        {
            selectable = true;
        }
        else
        {
            selectable = false;
        }

        sprite.color = selectable ? (selected ? selectedColor : blinkableColor) : unBlinkableColor;
    }

    public override IEnumerator BlinkSpecificReaction()
    {
        StartCoroutine(remoteSpeaker.PickupSpeaker());
        yield return null;
    }
}
