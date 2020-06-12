using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instrument : SwitchElement
{
    private AnimSynchronizer synchronizer;
    public AudioClip reliveSound;

    private AudioSource source;
    private int soundFlag;
    void Start()
    {
        HandlerStart();
        source = GetComponent<AudioSource>();
        ZoneHandler.Instance.reliveRemotlyChanged = true;
        synchronizer = GetComponent<AnimSynchronizer>();
        soundFlag = 0;
    }

    void Update()
    {
        HandlerUpdate();
        UpdateState();
    }

    private void UpdateState()
    {
        ZoneHandler.Instance.reliveRemotlyChanged = true;
        ZoneHandler.Instance.currentReliveProgression = GetVillageReliveProgression();
        animator.SetBool("Relived", active);
        if(active)
        {
            synchronizer.Synchronize();
            ZoneHandler.Instance.currentZone.isRelived = true;
            if(soundFlag >= 5)
            {
                soundFlag = 0;
                source.PlayOneShot(reliveSound);
            }
            foreach(HookState hook in GameManager.Instance.zoneHooks)
            {
                hook.Relive();
            }
        }
        else
        {
            ZoneHandler.Instance.currentZone.isRelived = false;
            soundFlag++;
        }
    }

    private float GetVillageReliveProgression()
    {
        int instrumentConverted = 0;

        if(WorldManager.GetWorldEvent(WorldManager.EventName.TambourRelived).occured)
        {
            instrumentConverted++;
        }
        if (WorldManager.GetWorldEvent(WorldManager.EventName.ViolonRelived).occured)
        {
            instrumentConverted++;
        }
        if (WorldManager.GetWorldEvent(WorldManager.EventName.FluteRelived).occured)
        {
            instrumentConverted++;
        }
        if (WorldManager.GetWorldEvent(WorldManager.EventName.SaxophoneRelived).occured)
        {
            instrumentConverted++;
        }

        return 0.25f * instrumentConverted;
    }
}
