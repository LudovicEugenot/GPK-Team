using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookFX : MonoBehaviour
{
    Animator animator;
    public ClassicHook hookFX;
    public TemporaryHook hookTemporaryFX;
    private SpriteMask mask;
    private SpriteRenderer targetRenderer;


    protected bool playerInSight;

    void Start()
    {
        targetRenderer = GetComponent<SpriteRenderer>();
        mask = GetComponent<SpriteMask>();
        animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
            animator.SetBool("IsExpend", true);
    }

    void LateUpdate()
    {
        if (mask.sprite != targetRenderer.sprite)
        {
            mask.sprite = targetRenderer.sprite;
        }
    }

    public bool PlayerInSight()
    {
        Vector2 playerDirection = transform.position - GameManager.Instance.player.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(GameManager.Instance.player.transform.position, playerDirection, playerDirection.magnitude, LayerMask.GetMask("Obstacle"));
        if(!hit)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
