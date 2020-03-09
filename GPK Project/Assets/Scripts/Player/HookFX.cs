using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookFX : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
     animator.Play("Expention_Hook");
    }

}
