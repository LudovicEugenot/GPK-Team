using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeHook : Hook
{
    public int storedLife;
    public Color usedColor;

    private bool used;

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
        if(!used)
        {
            GameManager.Instance.playerManager.Heal(storedLife);
            used = true;
        }
        yield return null;
    }

    public override void StateUpdate()
    {
        blinkable = Vector2.Distance(GameManager.Instance.blink.transform.position, transform.position) <= GameManager.Instance.blink.currentRange;


        sprite.color = blinkable ? (selected ? selectedColor : (used ? usedColor : blinkableColor)) : unselectableColor;
    }
}
