using System.Collections;
using System.Collections.Generic;
using UnityEngine;













public class BackgroundFX : MonoBehaviour
{
    public Material backgroundShader;
    public Material riverShader;
    public Material propsShader;

    [HideInInspector] public float recolor;

    void Update()
    {
        recolor = GameManager.Instance.zoneHandler.currentReliveProgression;
        backgroundShader.SetFloat("Vector1_Shaderbackground", recolor);
        riverShader.SetFloat("Vector1_Shaderbackground", recolor);
        propsShader.SetFloat("Vector1_Shaderbackground", recolor);
        Debug.Log(propsShader.GetFloat("Vector1_Shaderbackground"));
    }
}
