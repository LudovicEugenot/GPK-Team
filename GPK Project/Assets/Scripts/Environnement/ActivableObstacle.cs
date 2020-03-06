﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivableObstacle : SwitchElement
{
    public Color activeColor;
    public Color deactiveColor;

    private Collider2D obstacleCollider;
    private SpriteRenderer sprite;

    void Start()
    {
        HandlerStart();

        obstacleCollider = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandlerUpdate();
        UpdateCollision();
    }

    private void UpdateCollision()
    {
        if(active)
        {
            obstacleCollider.enabled = true;
            sprite.color = activeColor;
        }
        else
        {
            sprite.color = deactiveColor;
            obstacleCollider.enabled = false;
        }
    }
}
