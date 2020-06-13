using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sinea : MonoBehaviour
{
    public WorldManager.StoryStep storyStepToAppear;

    void Start()
    {
        if(WorldManager.currentStoryStep < storyStepToAppear)
        {
            Destroy(gameObject);
        }
    }
}
