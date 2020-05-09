using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEventUpdater : MonoBehaviour
{
    public bool zoneNameUnknownUntilRecolor;
    public WorldManager.StoryStep storyStepSkip;

    private void Start()
    {
        if(WorldManager.currentStoryStep == WorldManager.StoryStep.Tutorial)
        {
            WorldManager.currentStoryStep = storyStepSkip;
        }

        InvokeRepeating("UpdateStoryStep", 1.0f, 1.0f);
    }

    void UpdateStoryStep()
    {
        ChechVillageReliving();
        CheckTutorialEnd();
        CheckVillageArrival();
        UpdateZoneDiscovery();
    }

    private void ChechVillageReliving()
    {
        if ((ZoneHandler.Instance.currentZone.buildIndex == 6
            || ZoneHandler.Instance.currentZone.buildIndex == 7
            || ZoneHandler.Instance.currentZone.buildIndex == 8
            || ZoneHandler.Instance.currentZone.buildIndex == 9)
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
        if(ZoneHandler.Instance.currentZone.name == "Pied du grand Sole"
            && WorldManager.currentStoryStep == WorldManager.StoryStep.Tutorial)
        {
            Debug.Log("Le tutoriel est fini");
            WorldManager.currentStoryStep = WorldManager.StoryStep.GoingToVillage;
        }
    }

    private void CheckVillageArrival()
    {
        if (ZoneHandler.Instance.currentZone.name == "Entrée du village"
            && WorldManager.currentStoryStep == WorldManager.StoryStep.GoingToVillage)
        {
            Debug.Log("Nous sommes au village");
            WorldManager.currentStoryStep = WorldManager.StoryStep.ArrivedToVillage;
        }
    }

    private void UpdateZoneDiscovery()
    {
        if (zoneNameUnknownUntilRecolor && !ZoneHandler.Instance.currentZone.isRelived)
        {
            GameManager.Instance.zoneName = "???";
        }
    }
}
