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
    public Color unBlinkableColor;
    public Animator decorationAnimator;
    public bool drawTrajectoryLine;

    [HideInInspector] public bool selected;
    [HideInInspector] public bool selectable;
    [HideInInspector] public bool blinkable;
    [HideInInspector] public SpriteRenderer sprite;

    protected Animator animator;
    protected AnimSynchronizer animSynchronizer;
    [HideInInspector] public HookState hookState;
    protected AudioSource source;
    private LineRenderer trajectoryLine;
    private ParticleSystem relivedEffect;

    #endregion


    public void HandlerStart()
    {
        trajectoryLine = GetComponent<LineRenderer>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        animSynchronizer = GetComponentInChildren<AnimSynchronizer>();
        source = GetComponent<AudioSource>();
        selected = false;
        selectable = true;
        relivedEffect = GetComponentInChildren<ParticleSystem>();
        if(relivedEffect != null)
        {
            relivedEffect.Stop();
        }
        if(trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
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
            if(relivedEffect != null && !relivedEffect.isPlaying)
                relivedEffect.Play();
        }

        if(drawTrajectoryLine)
        {
            DrawTrajectory();
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

    private void DrawTrajectory()
    {
        if(blinkable)
        {
            trajectoryLine.enabled = true;
            Vector3[] linePos = new Vector3[2] {transform.position, GameManager.Instance.blink.currentHook.transform.position};
            trajectoryLine.SetPositions(linePos);
        }
        else
        {
            trajectoryLine.enabled = false;
        }
    }
}
