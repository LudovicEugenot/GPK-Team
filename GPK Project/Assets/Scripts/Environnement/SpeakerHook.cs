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

    public IEnumerator CreateMusicArea()
    {
        rangeVisualO.SetActive(true);
        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapCircle(transform.position, agressionRanges[GameManager.Instance.playerManager.currentPower], enemiFilter, colliders);
        if (colliders.Count > 0)
        {
            foreach (Collider2D collider in colliders)
            {
                EnemyBase enemy = collider.transform.parent.GetChild(0).GetComponent<EnemyBase>();
                enemy.TakeDamage();
            }
        }

        yield return new WaitForSeconds(agressionTime);
        rangeVisualO.SetActive(false);
    }
}
