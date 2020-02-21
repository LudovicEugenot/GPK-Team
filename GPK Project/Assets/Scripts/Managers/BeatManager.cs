using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    #region Initialization
    [Range(1, 400)] public float bpm;
    [Tooltip("L'intervalle de temps dont le joueur dispose pour effectuer son action et être en rythme.")]
    [Range(0f, 1f)] public float timingThreshold = 0.2f;
    [Range(-1f, 1f)] public float timingThresholdOffset;
    [Range(0f, 1f)] public float beatStartTimeOffset; // à supprimer pour l'adapter selon la machine qui joue la musique

    public float cameraBeatEffectLerpSpeed;
    public float cameraBeatEffectAmplitude;


    private bool musicStarted;
    private float beatTime;
    private float timeBeforeNextBeat;
    private float currentBeatProgression;
    private float nextBeatStartTime;
    private float offBeatStartTime;
    private float songStartTime;

    private AudioSource source;

    private float initialCameraSize;
    private Camera mainCamera;
    #endregion


    void Start()
    {
        source = GetComponent<AudioSource>();
        musicStarted = false;
        beatTime = 60 / bpm;

        mainCamera = Camera.main;
        initialCameraSize = mainCamera.orthographicSize;
    }

    private void Update()
    {
        if (!musicStarted && Input.GetButtonDown("Blink"))
        {
            StartMusic();

            nextBeatStartTime = (float)AudioSettings.dspTime + beatStartTimeOffset;
            offBeatStartTime = nextBeatStartTime;
        }

        if (musicStarted)
        {
            TimeCycle();
        }
    }

    /// <summary>
    /// Manages the time according to beats.
    /// </summary>
    private void TimeCycle()
    {
        if (nextBeatStartTime < (float)AudioSettings.dspTime)
        {
            nextBeatStartTime += beatTime;
            StartCoroutine(BeatEffect(1));
        }

        if (offBeatStartTime < (float)AudioSettings.dspTime - beatTime / 2)
        {
            offBeatStartTime += beatTime;
            //StartCoroutine(BeatEffect(0.2f));
        }

        timeBeforeNextBeat = nextBeatStartTime - (float)AudioSettings.dspTime;
        currentBeatProgression = timeBeforeNextBeat / beatTime;
    }

    private IEnumerator BeatEffect(float amplitude)
    {
        mainCamera.orthographicSize = initialCameraSize + cameraBeatEffectAmplitude * amplitude;
        while (mainCamera.orthographicSize > initialCameraSize + 0.01f)
        {
            mainCamera.orthographicSize -= cameraBeatEffectLerpSpeed * (mainCamera.orthographicSize - initialCameraSize) * Time.fixedDeltaTime * 50;
            yield return new WaitForFixedUpdate();
        }
        mainCamera.orthographicSize = initialCameraSize;
    }

    public bool OnBeat()
    {
        bool onBeat = false;

        /*if(timeBeforeNextBeat < timingThreshold / 2 || beatTime - timeBeforeNextBeat < timingThreshold / 2)
        {
            onBeat = true;
        }*/

        if ((float)AudioSettings.dspTime > offBeatStartTime - (timingThreshold / 2) + timingThresholdOffset && (float)AudioSettings.dspTime < offBeatStartTime + (timingThreshold / 2) + timingThresholdOffset)
        {
            return true;
        }

        return onBeat;
    }

    private void StartMusic()
    {
        musicStarted = true;

        source.Play();

        songStartTime = (float)AudioSettings.dspTime;
    }
}
