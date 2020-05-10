using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchElement : MonoBehaviour
{
    public bool enableState;
    [Range(0.0f, 30.0f)] public float beatsBeforeDeactivation;
    public bool stayEnable;
    public List<Hookterruptor> connectedHookterruptors;
    public bool needAllPressed;

    [Tooltip("False means that it will be activated by the event if it's not null")]
    public bool isInteractableByEvent;
    public WorldManager.EventName relatedEvent;
    private WorldManager.WorldEvent relatedWorldEvent;
    public WorldManager.EventName triggeredEvent;
    private WorldManager.WorldEvent triggeredWorldEvent;

    [HideInInspector] public bool isEnabled;
    protected bool active;
    private float currentRemainingActiveTime;
    protected Animator animator;

    public void HandlerStart()
    {
        active = !enableState;
        currentRemainingActiveTime = enableState ? beatsBeforeDeactivation * BeatManager.Instance.BeatTime + 0.1f : 0;
        relatedWorldEvent = WorldManager.GetWorldEvent(relatedEvent);
        triggeredWorldEvent = WorldManager.GetWorldEvent(triggeredEvent);
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
        if (beatsBeforeDeactivation * BeatManager.Instance.BeatTime + 0.1f > 0)
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

        bool elementEnabled;
        if(needAllPressed)
        {
            elementEnabled = true;
            foreach (Hookterruptor hookterruptor in connectedHookterruptors)
            {
                if (!hookterruptor.pressed)
                {
                    elementEnabled = false;
                }
            }
        }
        else
        {
            elementEnabled = false;
            foreach (Hookterruptor hookterruptor in connectedHookterruptors)
            {
                if (hookterruptor.pressed)
                {
                    elementEnabled = true;
                }
            }
        }

        if(elementEnabled && connectedHookterruptors.Count > 0)
        {
            isEnabled = true;
            triggeredWorldEvent.occured = true;
            if(!stayEnable)
            {
                currentRemainingActiveTime = beatsBeforeDeactivation * BeatManager.Instance.BeatTime + 0.1f;
            }
        }
        active = isEnabled ? enableState : !enableState;
    }

    #region Obsolete
    public void SwitchOn()
    {
        active = enableState;
        currentRemainingActiveTime = beatsBeforeDeactivation;
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

        if(enableState && beatsBeforeDeactivation > 0)
        {
            currentRemainingActiveTime = beatsBeforeDeactivation;
        }

        return active;
    }

    #endregion
}
