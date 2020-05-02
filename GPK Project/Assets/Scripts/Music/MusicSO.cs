using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Music Loop SO New file", menuName = "Music Scriptable Objects/Music Loop")]
public class MusicSO : ScriptableObject
{
    [Header("Music Related")]
    public AudioClip explorationLoop;
    public AudioClip combatLoop;
    [Space]
    [Range(1, 250)] public int bpm = 140;
    [Range(0f, 1f)] public double explorationBeatStartTimeOffset;
    [Range(0f, 1f)] public double combatBeatStartTimeOffset;

    [Space]
    [Tooltip("Les deux musiques peuvent s'interchanger sur les beats 2 et 4 au lieu de juste 4.")]
    public bool canSwitchOnBeat2 = false;

    [Header("Data Related")]
    [Tooltip("L'intervalle de temps dont le joueur dispose pour effectuer son action et être en rythme.")]
    [Range(0f, 1f)] public float timingThreshold = 0.2f;
    [Range(-1f, 1f)] public float timingThresholdOffset;
    [Range(0f, 1f)] public float minTimeForOnBeatValidation;
    [Space]
    [Range(0f, 1f)] public float cameraBeatEffectLerpSpeed = 0.5f;
    [Range(0f, 1f)] public float cameraBeatEffectAmplitude = 0.1f;
}