using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Music Loop SO New file", menuName = "Music Scriptable Object")]
public class MusicSO : ScriptableObject
{
    [Header("Music Related")]

    [Tooltip("La musique calme est jouée lorsqu'il n'y a pas d'ennemis.")]
    public AudioClip calmLoop;
    [Range(0f, 3f)] public double calmMusicStartTimeOffset;
    [Tooltip("Quand on quitte un break, on commence la musique calme à un de ces beats. La musique calme ne démarre au début que lorsqu'on transitionne à partir d'une autre musique ou qu'on arrive au bout de la boucle.")]
    [Range(0, 80)] public int numberOfBeatsUntilLoop;
    [Range(0, 50)] public int[] beatsToInsertCalmMusic;
    [Space]
    [Tooltip("Les Loops qui font toutes 4 beats et qui changent aléatoirement vers une autre loop.")]
    public AudioClip[] combatLoop;
    [Range(0f, 3f)] public double[] combatMusicStartTimeOffset;
    [Space]
    [Tooltip("Les drops font le passage entre musique calme et musique de combat.")]
    public AudioClip[] drop;
    [Range(0f, 3f)] public double[] dropMusicStartTimeOffset;
    [Space]
    [Tooltip("Les breaks sont déclenchés à chaque fin de combat et font la transition entre le combat et le calme (mais aussi combat vers combat).")]
    public AudioClip[] breaks;
    [Range(0f, 3f)] public double[] breakMusicStartTimeOffset;
    [Space]
    [Range(1, 250)] public int bpm = 140;

    [Space]
    [Tooltip("Les loops peuvent s'interchanger sur les beats 2 et 4 au lieu de juste 4.")]
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