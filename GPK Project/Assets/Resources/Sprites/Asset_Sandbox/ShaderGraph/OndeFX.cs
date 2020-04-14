using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OndeFX : MonoBehaviour
{
    public Material OndeMaterial;
    public float MaxAmount = 50f;

    [Range(0, 1)]
    public float Friction = .9f;

    private float Amount = 0f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            this.Amount = this.MaxAmount;
            Vector3 pos = Input.mousePosition;
            this.OndeMaterial.SetFloat("_CenterX", pos.x);
            this.OndeMaterial.SetFloat("_CenterY", pos.y);
        }

        this.OndeMaterial.SetFloat("_Amount", this.Amount);
        this.Amount *= this.Friction;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, this.OndeMaterial);
    }
}