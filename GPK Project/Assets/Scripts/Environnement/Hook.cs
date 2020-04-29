using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Hook : MonoBehaviour
{
    #region Initialization
    [Header("General Hook Options")]
    public bool isSecureHook;
    public Color blinkableColor;
    public Color selectedColor;
    public Color unselectableColor;
    public Animator decorationAnimator;

    public bool agressiveHook;
    public float[] agressionRanges;
    public GameObject rangeVisualO; // à remplacer par le mask de recoloration
    public float agressionTime;

    [HideInInspector] public bool selected;
    [HideInInspector] public bool blinkable;
    [HideInInspector] public SpriteRenderer sprite;

    protected ContactFilter2D enemiFilter = new ContactFilter2D();
    protected Animator animator;
    protected AnimSynchronizer animSynchronizer;
    [HideInInspector] public HookState hookState;

    #endregion


    public void HandlerStart()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        animSynchronizer = GetComponentInChildren<AnimSynchronizer>();

        selected = false;
        blinkable = true;

        enemiFilter.useTriggers = true;
        enemiFilter.SetLayerMask(LayerMask.GetMask("Enemi"));
    }


    public void HandlerUpdate()
    {
        StateUpdate();

        if (decorationAnimator != null)
        {
            decorationAnimator.SetBool("IsConvert", hookState.relived);
        }

        if(hookState.relived)
        {
            Synch();
        }
    }

    public abstract void StateUpdate();

    public IEnumerator BlinkReaction(bool isPlayerOnBeat)
    {
        StartCoroutine(BlinkSpecificReaction());

        if(isPlayerOnBeat)
        {
            hookState.Relive();
            if (agressiveHook)
            {
                rangeVisualO.transform.localScale = new Vector2(agressionRanges[GameManager.Instance.playerManager.currentPower], agressionRanges[GameManager.Instance.playerManager.currentPower]);
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
    }

    public abstract IEnumerator BlinkSpecificReaction();

    private void Synch()
    {
        if (animSynchronizer != null)
        {
            animSynchronizer.Synchronize();
        }
    }
}
