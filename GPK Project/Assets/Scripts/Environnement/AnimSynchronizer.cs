using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSynchronizer : MonoBehaviour
{
    //public AnimationClip anim;

    private Animator animator;
    //private int frameNumber;

    void Start()
    {
        animator = GetComponent<Animator>();
        //frameNumber = Mathf.RoundToInt(anim.length * anim.frameRate);
    }

    public void Synchronize()
    {
        //anim.frameRate = (float)frameNumber * GameManager.Instance.Beat.beatTime;
        animator.SetFloat("Speed", 1 / GameManager.Instance.Beat.beatTime);
    }
}
