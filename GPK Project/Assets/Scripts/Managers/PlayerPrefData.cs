using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerPrefData
{
    public float masterVolume;
    public float musicVolume;
    public float soundEffectsVolume;
    public float timingTresholdOffset;
    public bool usePlaytestRecord;

    public PlayerPrefData(GameManager gameManager)
    {
        masterVolume = gameManager.masterVolume;
        musicVolume = gameManager.musicVolume;
        soundEffectsVolume = gameManager.soundEffectsVolume;
        usePlaytestRecord = gameManager.usePlaytestRecord;
        timingTresholdOffset = gameManager.Beat.timingThresholdOffset;
    }
}
