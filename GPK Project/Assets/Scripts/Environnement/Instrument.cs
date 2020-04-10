using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instrument : SwitchElement
{
    public Color relivedColor;

    private SpriteRenderer spriteRenderer;
    private Color initialColor;

    void Start()
    {
        HandlerStart();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
    }

    void Update()
    {
        HandlerUpdate();
        UpdateState();
    }

    private void UpdateState()
    {
        if(active)
        {
            spriteRenderer.color = relivedColor;
        }
        else
        {
            spriteRenderer.color = initialColor;
        }
    }
}
