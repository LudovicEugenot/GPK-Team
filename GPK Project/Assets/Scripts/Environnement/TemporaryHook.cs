using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryHook : Hook
{
    public int beatRepairTime;
    public int beatTimeBeforeBroke;
    public float addedTBBTime;
    public Color brokenColor;

    private bool isBroken;
    private bool unstable;
    private float currentTimeBeforeRepair;

    void Start()
    {
        HandlerStart();
        currentTimeBeforeRepair = 0;
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
            StartCoroutine(GameManager.Instance.blink.RespawnPlayer());
        }
        else
        {
            unstable = true;
            yield return new WaitForSeconds(beatTimeBeforeBroke * GameManager.Instance.Beat.BeatTime + GameManager.Instance.Beat.timingThreshold / 2);
            if((Vector2)GameManager.Instance.blink.transform.parent.position == (Vector2)transform.position)
            {
                StartCoroutine(GameManager.Instance.blink.RespawnPlayer());
            }
            yield return new WaitForSeconds(addedTBBTime);
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

        if (Vector2.Distance(GameManager.Instance.blink.transform.position, transform.position) <= GameManager.Instance.blink.currentRange)
        {
            blinkable = true;
        }
        else
        {
            blinkable = false;
        }

        sprite.color = !isBroken ? (blinkable ? (selected ? selectedColor : blinkableColor) : unselectableColor) : brokenColor;
    }
}
