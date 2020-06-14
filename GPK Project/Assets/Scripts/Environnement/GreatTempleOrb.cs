using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
public class GreatTempleOrb : MonoBehaviour
{
    public WorldManager.EventName eventToLightUp;

    private WorldManager.WorldEvent worldEventToLightUp;
    private Animator animator;
    private Light2D orbLight;
    void Start()
    {
        orbLight = GetComponentInChildren<Light2D>();
        animator = GetComponent<Animator>();
        worldEventToLightUp = WorldManager.GetWorldEvent(eventToLightUp);
    }

    void Update()
    {
        animator.SetBool("LightenUp", worldEventToLightUp.occured);
        orbLight.enabled = worldEventToLightUp.occured;
    }
}
