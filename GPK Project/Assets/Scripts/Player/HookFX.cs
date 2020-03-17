using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookFX : MonoBehaviour
{
    Animator animator;
    public ClassicHook hookFX;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if(hookFX.converted == true)
        {
            animator.SetBool("IsExpend", true);
        }

    }

}
