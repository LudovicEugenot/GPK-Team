using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivableObstacle : SwitchElement
{
    public AudioClip upSound;
    public AudioClip downSound;

    private Collider2D obstacleCollider;
    private AudioSource source;

    void Start()
    {
        HandlerStart();

        source = GetComponent<AudioSource>();
        obstacleCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        HandlerUpdate();
        UpdateState();
    }

    private void UpdateState()
    {
        if(animator != null)
        {
            animator.SetBool("Activated", active);
        }

        if (active && obstacleCollider.enabled == false)
        {
            obstacleCollider.enabled = true;
            if (upSound != null)
                source.PlayOneShot(upSound);
        }

        if(!active && obstacleCollider.enabled == true)
        {
            obstacleCollider.enabled = false;
            if(downSound != null)
                source.PlayOneShot(downSound);
        }
    }
}
