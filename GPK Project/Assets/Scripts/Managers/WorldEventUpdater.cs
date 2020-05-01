using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEventUpdater : MonoBehaviour
{
    public WorldManager.StoryStep storyStepSkip;

    private void Start()
    {
        if(WorldManager.currentStoryStep == WorldManager.StoryStep.Tutorial)
        {
            WorldManager.currentStoryStep = storyStepSkip;
        }

        InvokeRepeating("UpdateStoryStep", 0.0f, 1.0f);
    }

    void UpdateStoryStep()
    {
        ChechVillageReliving();
        CheckTutorialEnd();
        CheckVillageArrival();
    }

    private void ChechVillageReliving()
    {
        if ((ZoneHandler.Instance.currentZone.name == "Village1"
            || ZoneHandler.Instance.currentZone.name == "Village2"
            || ZoneHandler.Instance.currentZone.name == "Village3"
            || ZoneHandler.Instance.currentZone.name == "Village4")
            && WorldManager.GetWorldEvent(WorldManager.EventName.TambourRelived).occured
            && WorldManager.GetWorldEvent(WorldManager.EventName.ViolonRelived).occured
            && WorldManager.GetWorldEvent(WorldManager.EventName.FluteRelived).occured
            && WorldManager.GetWorldEvent(WorldManager.EventName.SaxophoneRelived).occured
            && WorldManager.currentStoryStep == WorldManager.StoryStep.ArrivedToVillage)
        {
            Debug.Log("Le village est entièrement réveillé");
            WorldManager.currentStoryStep = WorldManager.StoryStep.VillageConverted;
        }
    }

    private void CheckTutorialEnd()
    {
        if(ZoneHandler.Instance.currentZone.name == "TrajetVillage1"
            && WorldManager.currentStoryStep == WorldManager.StoryStep.Tutorial)
        {
            Debug.Log("Le tutoriel est fini");
            WorldManager.currentStoryStep = WorldManager.StoryStep.GoingToVillage;
        }
    }

    private void CheckVillageArrival()
    {
        if (ZoneHandler.Instance.currentZone.name == "Village1"
            && WorldManager.currentStoryStep == WorldManager.StoryStep.GoingToVillage)
        {
            Debug.Log("Nous sommes au village");
            WorldManager.currentStoryStep = WorldManager.StoryStep.ArrivedToVillage;
        }
    }
}
