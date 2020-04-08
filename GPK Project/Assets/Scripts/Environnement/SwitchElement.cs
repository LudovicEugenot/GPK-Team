using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchElement : MonoBehaviour
{
    public bool enableState;
    public float timeBeforeDeactivation;
    public bool stayActive;

    public WorldManager.EventName relatedEvent;
    private WorldManager.WorldEvent relatedWorldEvent;

    [HideInInspector] public bool active;
    private float currentRemainingActiveTime;

    public void HandlerStart()
    {
        active = !enableState;
        currentRemainingActiveTime = enableState ? timeBeforeDeactivation : 0;
        relatedWorldEvent = WorldManager.GetWorldEvent(relatedEvent);
    }

    public void HandlerUpdate()
    {
        if(timeBeforeDeactivation > 0)
        {
            UpdateActiveTime();
        }

        if(relatedEvent != WorldManager.EventName.NullEvent)
        {
            if(relatedWorldEvent.occured)
            {
                active = enableState;
            }
        }
    }

    public void SwitchOn()
    {
        active = enableState;
        currentRemainingActiveTime = timeBeforeDeactivation;
    }

    public void SwitchOff()
    {
        if(!stayActive)
        {
            active = !enableState;
        }
    }

    public bool SwitchOnce()
    {
        if(!enableState || (enableState && !stayActive))
        {
            active = !active;
        }

        if(enableState && timeBeforeDeactivation > 0)
        {
            currentRemainingActiveTime = timeBeforeDeactivation;
        }

        return active;
    }

    private void UpdateActiveTime()
    {
        if(currentRemainingActiveTime > 0)
        {
            currentRemainingActiveTime -= Time.deltaTime;
        }
        else if(!stayActive)
        {
            active = !enableState;
        }
    }
}
