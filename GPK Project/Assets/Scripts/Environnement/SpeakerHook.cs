﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerHook : Hook
{
    [Header("Classic Hook Options")]
    public Animator animator;

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

    public override IEnumerator BlinkReaction()
    {
        //Animation récupération de la capacité versatile
        StartCoroutine(remoteSpeaker.PickupSpeaker());
        yield return null;
    }
}
