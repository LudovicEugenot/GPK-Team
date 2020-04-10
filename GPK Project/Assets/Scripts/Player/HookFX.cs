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

}
