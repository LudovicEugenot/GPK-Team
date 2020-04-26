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
    [Range(0f, 1f)] public float minTimeForOnBeatValidation;

    public float cameraBeatEffectLerpSpeed;
    public float cameraBeatEffectAmplitude;

    [HideInInspector] public bool onBeatSingleFrame;
    [HideInInspector] public bool onBeatFirstFrame;
    [HideInInspector] public bool onBeatNextFrame;
    private bool firstFrameFlag;
    private bool nextFrameFlag;

    private bool musicStarted;
    private double _beatTime;
    public float BeatTime
    {
        get
        {
            return (float)_beatTime;
        }
    }
    private double timeBeforeNextBeat;
    [HideInInspector] public float currentBeatProgression;
    [HideInInspector] public bool useCameraBeatShake;
    private double nextBeatStartTime;
    private double offBeatStartTime;
    private double songStartTime;
    private double audioDspTimeDelay;
    private double pauseStartTime;
    private double audioPlayTime;
    private bool beatActionUsed;
    private double lastActionTime;
    private bool actOnBeatPossible;

    private AudioSource source;

    private float initialCameraSize;
    #endregion

    #region Singleton
    public static BeatManager Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    #endregion

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        source = GetComponent<AudioSource>();
        musicStarted = false;
        _beatTime = 60 / bpm;
        onBeatSingleFrame = false;
        beatActionUsed = false;

        initialCameraSize = Camera.main.orthographicSize;
        audioDspTimeDelay = 0;
        useCameraBeatShake = true;
    }

    private void Update()
    {
        if (!musicStarted)
        {
            StartMusic();

            nextBeatStartTime = beatStartTimeOffset;
            offBeatStartTime = nextBeatStartTime;
        }

        audioPlayTime = (float)AudioSettings.dspTime - songStartTime - audioDspTimeDelay;
        if (musicStarted && !GameManager.Instance.paused)
        {
            TimeCycle();
        }

        if(onBeatNextFrame && !beatActionUsed)
        {
            GameManager.Instance.blink.FailCombo();
        }
    }

    /// <summary>
    /// Manages the time according to beats.
    /// </summary>
    private void TimeCycle()
    {
        if (onBeatSingleFrame)
        {
            onBeatSingleFrame = false;
        }

        if (onBeatFirstFrame)
        {
            onBeatFirstFrame = false;
        }

        if (onBeatNextFrame)
        {
            onBeatNextFrame = false;
        }

        if (OnBeat(false))
        {
            if(firstFrameFlag)
            {
                onBeatFirstFrame = true;
                firstFrameFlag = false;
            }

            nextFrameFlag = true;
        }
        else
        {
            firstFrameFlag = true;

            if(nextFrameFlag)
            {
                onBeatNextFrame = true;
                nextFrameFlag = false;
            }
        }

        if (nextBeatStartTime < audioPlayTime)
        {
            nextBeatStartTime += _beatTime;
            if(cameraBeatEffectAmplitude != 0 && useCameraBeatShake)
            {
                StartCoroutine(BeatEffect(1.0f));
            }
            onBeatSingleFrame = true;
        }

        if (offBeatStartTime < audioPlayTime - _beatTime / 2)
        {
            offBeatStartTime += _beatTime;
            /*if (cameraBeatEffectAmplitude != 0 && useCamearBeatShake)
            {
                StartCoroutine(BeatEffect(0.2f));
            }*/
            beatActionUsed = false;
        }

        timeBeforeNextBeat = nextBeatStartTime - audioPlayTime;
        currentBeatProgression = (float)(1 - (timeBeforeNextBeat / _beatTime));
    }

    private IEnumerator BeatEffect(float amplitude)
    {
        Camera.main.orthographicSize = initialCameraSize + cameraBeatEffectAmplitude * amplitude;
        while (Camera.main.orthographicSize > initialCameraSize + 0.01f)
        {
            Camera.main.orthographicSize -= cameraBeatEffectLerpSpeed * (Camera.main.orthographicSize - initialCameraSize) * Time.fixedDeltaTime * 50;
            yield return new WaitForFixedUpdate();
        }
        Camera.main.orthographicSize = initialCameraSize;
    }

    public bool OnBeat(bool isAction)
    {
        bool onBeat = false;
        double _beatTimeProgression = _beatTime - timeBeforeNextBeat;

        if(actOnBeatPossible || !isAction)
        {
            if (timingThresholdOffset >= timingThreshold / 2)
            {
                if (_beatTimeProgression < (timingThreshold / 2) + timingThresholdOffset && _beatTimeProgression > timingThresholdOffset - (timingThreshold / 2))
                {
                    onBeat = true;
                }
            }
            else if (timingThresholdOffset <= -timingThreshold / 2)
            {
                if (timeBeforeNextBeat < (timingThreshold / 2) - timingThresholdOffset && timeBeforeNextBeat > -timingThresholdOffset - (timingThreshold / 2))
                {
                    onBeat = true;
                }
            }
            else
            {
                if (_beatTimeProgression < (timingThreshold / 2) + timingThresholdOffset || timeBeforeNextBeat < (timingThreshold / 2) - timingThresholdOffset)
                {
                    onBeat = true;
                }
            }
        }

        return onBeat;
    }

    public bool CanAct()
    {
        bool used = beatActionUsed;
        if(!beatActionUsed)
        {
            beatActionUsed = true;
        }

        actOnBeatPossible = false;
        if (audioPlayTime - lastActionTime > minTimeForOnBeatValidation)
        {
            actOnBeatPossible = true;
        }

        lastActionTime = audioPlayTime;

        return !used;
    }

    private void StartMusic()
    {
        musicStarted = true;

        source.Play();

        songStartTime = (float)AudioSettings.dspTime;
    }

    public void PauseMusic()
    {
        if (source != null)
            source.Pause();
        pauseStartTime = audioPlayTime;
    }

    public void UnPauseMusic()
    {
        if(source != null)
            source.UnPause();

        audioDspTimeDelay += audioPlayTime - pauseStartTime;
    }
}
