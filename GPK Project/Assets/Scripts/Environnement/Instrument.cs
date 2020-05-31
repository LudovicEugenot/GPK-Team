using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instrument : SwitchElement
{
    private AnimSynchronizer synchronizer;

    void Start()
    {
        HandlerStart();
        ZoneHandler.Instance.reliveRemotlyChanged = true;
        synchronizer = GetComponent<AnimSynchronizer>();
    }

    void Update()
    {
        HandlerUpdate();
        UpdateState();
    }

    private void UpdateState()
    {
        ZoneHandler.Instance.reliveRemotlyChanged = true;
        animator.SetBool("Relived", active);
        if(active)
        {
            synchronizer.Synchronize();
            ZoneHandler.Instance.currentReliveProgression = 1;
            ZoneHandler.Instance.currentZone.isRelived = true;
            foreach(HookState hook in GameManager.Instance.zoneHooks)
            {
                hook.Relive();
            }
        }
        else
        {
            ZoneHandler.Instance.currentReliveProgression = 0;
            ZoneHandler.Instance.currentZone.isRelived = false;
        }
    }
}
