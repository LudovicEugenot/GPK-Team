using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeHook : Hook
{
    public int storedLife;
    public Color usedColor;
    public int beatTimeBewteenHeals;

    private bool used;
    private float beatTimeBeforeNextHeal;

    private void Start()
    {
        HandlerStart();
    }

    private void Update()
    {
        HandlerUpdate();
    }

    public override IEnumerator BlinkSpecificReaction()
    {
        if(!used && ZoneHandler.Instance.currentReliveProgression < 1)
        {
            GameManager.Instance.playerManager.Heal(storedLife);
            used = true;
        }
        yield return null;
    }

    public override void StateUpdate()
    {
        if (ZoneHandler.Instance.currentReliveProgression == 1 && GameManager.Instance.blink.currentHook == this)
        {
            if(BeatManager.Instance.onBeatSingleFrame)
            {
                if (beatTimeBeforeNextHeal > 1)
                {
                    beatTimeBeforeNextHeal--;
                }
                else
                {
                    beatTimeBeforeNextHeal = beatTimeBewteenHeals;
                    GameManager.Instance.playerManager.Heal(1);
                }
            }
        }



        blinkable = Vector2.Distance(GameManager.Instance.blink.transform.position, transform.position) <= GameManager.Instance.blink.currentRange && PlayerInSight();


        sprite.color = blinkable ? (selected ? selectedColor : (ZoneHandler.Instance.currentReliveProgression == 1 ? blinkableColor : (used ? usedColor : blinkableColor))) : unselectableColor;
    }
}
