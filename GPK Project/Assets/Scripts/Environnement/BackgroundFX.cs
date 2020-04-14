using System.Collections;
using System.Collections.Generic;
using UnityEngine;


















public class BackgroundFX : MonoBehaviour
{
    public Material recolorMaterial;
    [Range(0f,1f)]public float lerpSpeed;
    [Range(0f, 1f)] public float maxRecolorNotFull;
    [Range(0f, 1f)] public float startRecolorValue;

    [HideInInspector] public float recolor;
    private float targetRecolorValue;
    private float lerpStep;

    private void Start()
    {
        recolor = 0;
    }
    void Update()
    {
        if(GameManager.Instance.zoneHandler != null)
        {
            targetRecolorValue = GameManager.Instance.zoneHandler.currentReliveProgression;
        }
        else
        {
            targetRecolorValue = 0;
        }
        targetRecolorValue = targetRecolorValue == 1 ? 1 : targetRecolorValue * maxRecolorNotFull;

        if (recolor != targetRecolorValue)
        {
            if (lerpStep <= 0.99f)
            {
                lerpStep += (1 - lerpStep) * lerpSpeed * Time.deltaTime;
                recolor += (targetRecolorValue - recolor) * lerpStep;
            }
            else
            {
                recolor = targetRecolorValue;
                lerpStep = 0;
            }
        }
        recolorMaterial.SetFloat("Vector1_Progression", startRecolorValue + recolor * (1 - startRecolorValue));
    }

    private IEnumerator FinishRecolor()
    {
        yield return null;
    }
}
