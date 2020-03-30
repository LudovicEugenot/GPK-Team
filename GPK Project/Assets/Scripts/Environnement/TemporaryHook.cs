using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryHook : Hook
{
    public int beatRepairTime;
    public int beatTimeBeforeBroke;
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

    public override IEnumerator BlinkSpecificReaction()
    {
        if(isBroken)
        {
            StartCoroutine(GameManager.Instance.blink.RespawnPlayer());
        }
        else
        {
            yield return new WaitForSeconds(beatTimeBeforeBroke * GameManager.Instance.Beat.beatTime + GameManager.Instance.Beat.timingThreshold / 2);
            isBroken = true;
            if((Vector2)GameManager.Instance.blink.transform.parent.position == (Vector2)transform.position)
            {
                StartCoroutine(GameManager.Instance.blink.RespawnPlayer());
            }
        }
    }

    public override void StateUpdate()
    {
        if(isBroken)
        {
            if(currentTimeBeforeRepair < beatRepairTime * GameManager.Instance.Beat.beatTime)
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
