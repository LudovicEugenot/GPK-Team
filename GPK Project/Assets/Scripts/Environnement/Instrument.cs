using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instrument : SwitchElement
{
    void Start()
    {
        HandlerStart();
        ZoneHandler.Instance.isInstrumentPresent = true;
    }

    void Update()
    {
        HandlerUpdate();
        UpdateState();
    }

    private void UpdateState()
    {
        ZoneHandler.Instance.isInstrumentPresent = true;
        animator.SetBool("Relived", active);
        if(active)
        {
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
        }
    }
}
