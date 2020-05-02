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

    [HideInInspector] public bool selected;
    [HideInInspector] public bool blinkable;
    [HideInInspector] public SpriteRenderer sprite;

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
        }
        yield return null;
    }

    public abstract IEnumerator BlinkSpecificReaction();

    private void Synch()
    {
        if (animSynchronizer != null)
        {
            animSynchronizer.Synchronize();
        }
    }

    protected bool PlayerInSight()
    {
        Vector2 playerDirection = transform.position - GameManager.Instance.player.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(GameManager.Instance.player.transform.position, playerDirection, playerDirection.magnitude, LayerMask.GetMask("Obstacle"));
        if (!hit)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
