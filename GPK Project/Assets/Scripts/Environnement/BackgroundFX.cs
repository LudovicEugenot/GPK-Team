using System.Collections;
using System.Collections.Generic;
using UnityEngine;


















public class BackgroundFX : MonoBehaviour
{
    public Material backgroundShader;
    public Material riverShader;
    public Material propsShader;
    [Range(0f,1f)]public float lerpSpeed;

    [HideInInspector] public float recolor;
    private float lerpStep;

    private void Start()
    {
        recolor = 0;
    }
    void Update()
    {
        if (lerpStep <= 0.95)
        {
            lerpStep += (1 - lerpStep) * lerpSpeed;
            recolor += (GameManager.Instance.zoneHandler.currentReliveProgression - recolor) * lerpStep;
        }
        backgroundShader.SetFloat("Vector1_ShaderBackground", recolor);
        riverShader.SetFloat("Vector1_ShaderBackground", recolor);
        propsShader.SetFloat("Vector1_ShaderBackground", recolor);
    }
}
