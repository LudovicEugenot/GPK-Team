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
            blinkable = true;
        }
        else
        {
            blinkable = false;
        }

        sprite.color = blinkable ? (selected ? selectedColor : blinkableColor) : unselectableColor;
    }

    public override IEnumerator BlinkSpecificReaction()
    {
        //Animation récupération de la capacité versatile
        StartCoroutine(remoteSpeaker.PickupSpeaker());
        yield return null;
    }
}
