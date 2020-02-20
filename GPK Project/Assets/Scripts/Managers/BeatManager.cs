using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    #region Initialization
    public int bpm;
    [Tooltip("L'intervalle de temps dont le joueur dispose pour effectuer son action et être en rythme.")]
    [Range(0f,1f)] public float timingThreshold = 0.2f;
    [Range(-1f,1f)] public float timingThresholdOffset;

    private float beatTime;
    private float timeBeforeNextBeat;
    private float nextBeatStartTime;
    #endregion


    void Start()
    {
        beatTime = 60 / bpm;
        timeBeforeNextBeat = beatTime;
        nextBeatStartTime = Time.time;
    }


    void Update()
    {
        TimeCycle();
    }

    /// <summary>
    /// Manages the time according to beats.
    /// </summary>
    private void TimeCycle()
    {
        if (nextBeatStartTime < Time.time)
        {
            nextBeatStartTime += beatTime;
        }
        timeBeforeNextBeat = nextBeatStartTime - Time.time;
    }
}
