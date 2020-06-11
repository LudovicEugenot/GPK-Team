using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oros : MonoBehaviour
{
    public WorldManager.StoryStep storyStepToRelive;
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
        if (ZoneHandler.Instance.zoneInitialized)
        {
            if (!relived && WorldManager.currentStoryStep >= storyStepToRelive)
            {
                relived = true;
                animator.SetBool("Relive", true);
                synchronizer.Synchronize();
            }
            else if (relived && WorldManager.currentStoryStep < storyStepToRelive)
            {
                relived = false;
                animator.SetBool("Relive", false);
            }
        }
    }
}
