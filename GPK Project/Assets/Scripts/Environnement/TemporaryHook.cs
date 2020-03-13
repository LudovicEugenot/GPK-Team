using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryHook : Hook
{
    public float repairTime;
    public float timeBeforeBroke;
    public Color brokenColor;

    private bool isBroken;
    private float currentTimeBeforeRepair;

    void Start()
    {
        HandlerStart();
        currentTimeBeforeRepair = 0;
    }

    void Update()
    {
        HandlerUpdate();
    }

    public override IEnumerator BlinkReaction()
    {
        if(isBroken)
        {
            blink.RespawnPlayer();
        }
        else
        {
            yield return new WaitForSeconds(timeBeforeBroke);
            isBroken = true;
        }
    }

    public override void StateUpdate()
    {
        if(isBroken)
        {
            if(currentTimeBeforeRepair < repairTime)
            {
                currentTimeBeforeRepair += Time.deltaTime;
            }
            else
            {
                isBroken = false;
                currentTimeBeforeRepair = 0;
            }
        }

        if (Vector2.Distance(blink.transform.position, transform.position) <= blink.currentRange && !isBroken)
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
