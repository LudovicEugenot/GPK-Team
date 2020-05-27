using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pnj : MonoBehaviour
{
    [HideInInspector] public bool relived;
    private Animator animator;
    private AnimSynchronizer synchronizer;

    void Start()
    {
        animator = GetComponent<Animator>();
        synchronizer = GetComponent<AnimSynchronizer>();
    }

    void Update()
    {
        if(!relived && ZoneHandler.Instance.currentZone.isRelived)
        {
            relived = true;
            animator.SetBool("Relive",true);
            synchronizer.Synchronize();
        }
        else if(relived && !ZoneHandler.Instance.currentZone.isRelived)
        {
            relived = false;
            animator.SetBool("Relive", false);
        }
    }
}
