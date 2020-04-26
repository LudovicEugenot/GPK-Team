using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchElement : MonoBehaviour
{
    public bool enableState;
    [Range(0.01f, 10)] public float timeBeforeDeactivation;
    public bool stayEnable;
    public List<Hookterruptor> connectedHookterruptors;


    [Tooltip("False means that it will be activated by the event if it's not null")]
    public bool isInteractableByEvent;
    public WorldManager.EventName relatedEvent;
    private WorldManager.WorldEvent relatedWorldEvent;

    [HideInInspector] public bool isEnabled;
    protected bool active;
    private float currentRemainingActiveTime;
    protected Animator animator;

    public void HandlerStart()
    {
        active = !enableState;
        currentRemainingActiveTime = enableState ? timeBeforeDeactivation : 0;
        relatedWorldEvent = WorldManager.GetWorldEvent(relatedEvent);
        animator = GetComponent<Animator>();
    }

    public void HandlerUpdate()
    {
        UpdateEnableState();

        if (relatedEvent != WorldManager.EventName.NullEvent && !isInteractableByEvent)
        {
            if (relatedWorldEvent.occured)
            {
                isEnabled = true;
            }
        }
    }

    private void UpdateEnableState()
    {
        if (timeBeforeDeactivation > 0)
        {
            if (currentRemainingActiveTime > 0)
            {
                currentRemainingActiveTime -= Time.deltaTime;
            }
            else if (!stayEnable)
            {
                isEnabled = false;
            }
        }

        bool elementEnabled = true;
        foreach(Hookterruptor hookterruptor in connectedHookterruptors)
        {
            if(!hookterruptor.pressed)
            {
                elementEnabled = false;
            }
        }

        if(elementEnabled && connectedHookterruptors.Count > 0)
        {
            isEnabled = true;
            if(!stayEnable)
            {
                currentRemainingActiveTime = timeBeforeDeactivation;
            }
        }
        active = isEnabled ? enableState : !enableState;
    }

    #region Obsolete
    public void SwitchOn()
    {
        active = enableState;
        currentRemainingActiveTime = timeBeforeDeactivation;
    }

    public void SwitchOff()
    {
        if(!stayEnable)
        {
            active = !enableState;
        }
    }

    public bool SwitchOnce()
    {
        if(!enableState || (enableState && !stayEnable))
        {
            active = !active;
        }

        if(enableState && timeBeforeDeactivation > 0)
        {
            currentRemainingActiveTime = timeBeforeDeactivation;
        }

        return active;
    }

    #endregion
}
