using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryHook : Hook
{
    public int beatRepairTime;
    public int beatTimeBeforeBroke;
    public float addedTBBTime;
    public Color brokenColor;
    public AudioClip breakSound;

    private bool isBroken;
    private bool unstable;
    private float currentTimeBeforeRepair;
    private TransitionManager.TransitionHook transitionHook;

    void Start()
    {
        HandlerStart();
        currentTimeBeforeRepair = 0;
        foreach (TransitionManager.TransitionHook tHook in GameManager.Instance.transitionHooks)
        {
            if (tHook.hook == this)
            {
                transitionHook = tHook;
            }
        }
    }

    void Update()
    {
        HandlerUpdate();

        animator.SetBool("Broken", isBroken);
        animator.SetBool("Unstable", unstable);
    }

    public override IEnumerator BlinkSpecificReaction()
    {
        if(isBroken)
        {
            FallEffect();
        }
        else
        {
            unstable = true;
            yield return new WaitForSeconds(beatTimeBeforeBroke * GameManager.Instance.Beat.BeatTime + GameManager.Instance.Beat.timingThreshold / 6);
            if(GameManager.Instance.blink.currentHook == this)
            {
                FallEffect();
            }
            yield return new WaitForSeconds(addedTBBTime);
            source.PlayOneShot(breakSound);
            unstable = false;
            isBroken = true;
        }
    }

    public override void StateUpdate()
    {
        if(isBroken)
        {
            if(currentTimeBeforeRepair < beatRepairTime * GameManager.Instance.Beat.BeatTime)
            {
                currentTimeBeforeRepair += Time.deltaTime;
            }
            else
            {
                isBroken = false;
                currentTimeBeforeRepair = 0;
            }
        }

        if (Vector2.Distance(GameManager.Instance.blink.transform.position, transform.position) <= GameManager.Instance.blink.currentRange && PlayerInSight())
        {
            blinkable = true;
        }
        else
        {
            blinkable = false;
        }

        sprite.color = !isBroken ? (blinkable ? (selected ? selectedColor : blinkableColor) : unselectableColor) : brokenColor;
    }

    private void FallEffect()
    {
        if (transitionHook == null)
        {
            StartCoroutine(GameManager.Instance.blink.RespawnPlayer());
        }
        else
        {
            TransitionManager.Instance.StartSecretTransition(transitionHook);
        }
    }
}
