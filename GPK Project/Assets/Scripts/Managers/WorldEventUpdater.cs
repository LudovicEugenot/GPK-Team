using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEventUpdater : MonoBehaviour
{
    public bool zoneNameUnknownUntilRecolor;
    public WorldManager.StoryStep cursedUntilStoryStep;
    public WorldManager.StoryStep storyStepSkip;
    public Talk warningTempleOpen;
    public bool talkTriggered;

    private void Start()
    {
        if(WorldManager.currentStoryStep == WorldManager.StoryStep.Tutorial)
        {
            WorldManager.currentStoryStep = storyStepSkip;
            Debug.LogWarning("The story step has been skipped to : " + WorldManager.currentStoryStep);
        }

        InvokeRepeating("UpdateStoryStep", 1.0f, 1.0f);
    }

    void UpdateStoryStep()
    {
        ChechVillageReliving();
        CheckTutorialEnd();
        CheckVillageArrival();
        UpdateZoneDiscovery();
        UpdateZoneCurse();
        CheckGreatInstrumentReliving();
        CheckTempleEntering();
        CheckKeyPossession();
        CheckBossState();
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
        if(ZoneHandler.Instance.currentZone.buildIndex == 4 
            && WorldManager.currentStoryStep == WorldManager.StoryStep.Tutorial)
        {
            Debug.Log("Le tutoriel est fini");
            WorldManager.currentStoryStep = WorldManager.StoryStep.GoingToVillage;
        }
    }

    private void CheckVillageArrival()
    {
        if (ZoneHandler.Instance.currentZone.buildIndex == 6 
            && WorldManager.currentStoryStep == WorldManager.StoryStep.GoingToVillage)
        {
            Debug.Log("Nous sommes au village");
            WorldManager.currentStoryStep = WorldManager.StoryStep.ArrivedToVillage;
        }
    }

    private void CheckGreatInstrumentReliving()
    {
        int giRelived = 0;
        if (WorldManager.GetWorldEvent(WorldManager.EventName.StringInstrumentRelived).occured)
        {
            giRelived++;
        }
        if (WorldManager.GetWorldEvent(WorldManager.EventName.RythmInstrumentRelived).occured)
        {
            giRelived++;
        }
        if (WorldManager.GetWorldEvent(WorldManager.EventName.VoiceInstrumentRelived).occured)
        {
            giRelived++;
        }

        if(giRelived == 3 && WorldManager.currentStoryStep < WorldManager.StoryStep.AllInstrumentRelived)
        {
            Debug.Log("Tous les instrument sont reactivés");
            GameManager.Instance.dialogueManager.StartTalk(warningTempleOpen, GameManager.Instance.player.transform.position, 3);
            WorldManager.currentStoryStep = WorldManager.StoryStep.AllInstrumentRelived;
        }
        else if(giRelived == 2 && WorldManager.currentStoryStep < WorldManager.StoryStep.SecondInstrumentRelived)
        {
            Debug.Log("Le deuxième instrument est reactivé");
            WorldManager.currentStoryStep = WorldManager.StoryStep.SecondInstrumentRelived;
        }
        else if (giRelived == 1 && WorldManager.currentStoryStep < WorldManager.StoryStep.FirstInstrumentRelived)
        {
            Debug.Log("Le premier instrument est reactivé");
            WorldManager.currentStoryStep = WorldManager.StoryStep.FirstInstrumentRelived;
        }
    }

    private void CheckTempleEntering()
    {
        if (ZoneHandler.Instance.currentZone.buildIndex == 28
            && WorldManager.currentStoryStep == WorldManager.StoryStep.AllInstrumentRelived)
        {
            Debug.Log("Nous sommes dans le temple !");
            WorldManager.currentStoryStep = WorldManager.StoryStep.EnteredTemple;
        }
    }

    private void CheckKeyPossession()
    {
        if (WorldManager.GetWorldEvent(WorldManager.EventName.DungeonKeyLoot).occured &&
            WorldManager.currentStoryStep < WorldManager.StoryStep.KeyObtained)
        {
            Debug.Log("La clé est récupérée");
            WorldManager.currentStoryStep = WorldManager.StoryStep.KeyObtained;
        }

    }

    
    // Check pour voir si le joueur a fini le jeu.
    private void CheckBossState()
    {
        if (WorldManager.GetWorldEvent(WorldManager.EventName.BossBeaten).occured &&
            WorldManager.currentStoryStep < WorldManager.StoryStep.EndGame)
        {
            Debug.Log("Le boss est vaincu!");
            WorldManager.currentStoryStep = WorldManager.StoryStep.EndGame;
        }
    }


    private void UpdateZoneCurse()
    {
        if(WorldManager.currentStoryStep >= cursedUntilStoryStep)
        {
            //ZoneHandler.Instance.reliveRemotlyChanged = false;
        }
        else
        {
            ZoneHandler.Instance.reliveRemotlyChanged = true;
            ZoneHandler.Instance.currentReliveProgression = 0;
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
