using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pianist : MonoBehaviour
{
    public WorldManager.StoryStep storyStepToPlay;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.P))
        {
            WorldManager.currentStoryStep = WorldManager.StoryStep.VillageConverted;
        }

        if (storyStepToPlay != WorldManager.StoryStep.Tutorial && WorldManager.currentStoryStep >= storyStepToPlay)
        {
            animator.SetTrigger("Play");
        }
    }
}
