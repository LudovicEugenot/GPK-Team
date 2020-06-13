using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSynchronizer : MonoBehaviour
{
    private Animator animator;

    public void Synchronize()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        Debug.Log(gameObject.name + " synchronize");
        animator.SetFloat("Speed", BeatManager.Instance.bpm > 120 ? 1 / BeatManager.Instance.BeatTime : 2 / BeatManager.Instance.BeatTime);
    }
}
