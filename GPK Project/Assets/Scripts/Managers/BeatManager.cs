using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    #region Initialization
    [Range(1, 400)] public float bpm;
    [Tooltip("L'intervalle de temps dont le joueur dispose pour effectuer son action et être en rythme.")]
    [Range(0f, 1f)] public float timingThreshold = 0.2f;
    [Range(-1f, 1f)] public float timingThresholdOffset;
    [Range(0f, 1f)] public float beatStartTimeOffset;
    [Range(0f, 1f)] public float minTimeForOnBeatValidation;

    public float cameraBeatEffectLerpSpeed;
    public float cameraBeatEffectAmplitude;

    [HideInInspector] public bool onBeatSingleFrame;
    [HideInInspector] public bool onBeatFirstFrame;
    [HideInInspector] public bool onBeatNextFrame;
    private bool firstFrameFlag;
    private bool nextFrameFlag;

    private bool musicStarted;
    [HideInInspector] public float beatTime;
    private float timeBeforeNextBeat;
    [HideInInspector] public float currentBeatProgression;
    private float nextBeatStartTime;
    private float offBeatStartTime;
    private double songStartTime;
    private bool beatActionUsed;
    private float lastActionTime;
    private bool actOnBeatPossible;

    private AudioSource switchingSource;
    private AudioSource otherSource;
    private AudioSource source1;
    private AudioSource source2;

    private float initialCameraSize;
    #endregion


    #region Singleton
    public static BeatManager Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (Instance == null)
        {
            Instance = this;

            AudioSource[] sources = GetComponents<AudioSource>();
            source1 = sources[0];
            source2 = sources[1];
            switchingSource = source1;
        }
        else
            Destroy(this.gameObject);
    }
    #endregion

    void Start()
    {
        musicStarted = false;
        MusicInit();

        initialCameraSize = Camera.main.orthographicSize;
    }

    private void Update()
    {
        if (!musicStarted)
        {
            StartMusic();

            nextBeatStartTime = (float)AudioSettings.dspTime + beatStartTimeOffset;
            offBeatStartTime = nextBeatStartTime;
        }

        if (musicStarted)
        {
            TimeCycle();
        }

        if (onBeatNextFrame && !beatActionUsed)
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
            if (firstFrameFlag)
            {
                onBeatFirstFrame = true;
                firstFrameFlag = false;
            }

            nextFrameFlag = true;
        }
        else
        {
            firstFrameFlag = true;

            if (nextFrameFlag)
            {
                onBeatNextFrame = true;
                nextFrameFlag = false;
            }
        }

        if (nextBeatStartTime < (float)AudioSettings.dspTime)
        {
            nextBeatStartTime += beatTime;
            if (cameraBeatEffectAmplitude != 0)
            {
                StartCoroutine(BeatEffect(1.0f));
            }
            onBeatSingleFrame = true;
        }

        if (offBeatStartTime < (float)AudioSettings.dspTime - beatTime / 2)
        {
            offBeatStartTime += beatTime;
            /*if (cameraBeatEffectAmplitude != 0)
            {
                StartCoroutine(BeatEffect(0.2f));
            }*/
            beatActionUsed = false;
        }

        timeBeforeNextBeat = nextBeatStartTime - (float)AudioSettings.dspTime;
        currentBeatProgression = 1 - (timeBeforeNextBeat / beatTime);
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
        float beatTimeProgression = beatTime - timeBeforeNextBeat;

        if (actOnBeatPossible || !isAction)
        {
            if (timingThresholdOffset >= timingThreshold / 2)
            {
                if (beatTimeProgression < (timingThreshold / 2) + timingThresholdOffset && beatTimeProgression > timingThresholdOffset - (timingThreshold / 2))
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
                if (beatTimeProgression < (timingThreshold / 2) + timingThresholdOffset || timeBeforeNextBeat < (timingThreshold / 2) - timingThresholdOffset)
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
        if (!beatActionUsed)
        {
            beatActionUsed = true;
        }

        actOnBeatPossible = false;
        if ((float)AudioSettings.dspTime - lastActionTime > minTimeForOnBeatValidation)
        {
            actOnBeatPossible = true;
        }

        lastActionTime = (float)AudioSettings.dspTime;

        return !used;
    }

    private void StartMusic()
    {
        musicStarted = true;

        switchingSource.Play();

        songStartTime = AudioSettings.dspTime;
    }

    private void MusicInit()
    {
        beatTime = 60 / bpm;
        onBeatSingleFrame = false;
        beatActionUsed = false;
    }

    public void LoadMusic(AudioClip clip)
    {
        switchingSource.clip = clip;
        switchingSource.Play();
    }

    public void LoadMusic(AudioClip clip, double timeUntilStart)
    {
        SwitchSource();
        switchingSource.clip = clip;
        switchingSource.PlayScheduled(AudioSettings.dspTime + timeUntilStart);
        StartCoroutine(StopMusic(otherSource, (float)timeUntilStart));
    }

    public void PlayThisClipAtThisTimer(AudioClip clip, double timerUntilPlayed, double timerEntryMusic)
    {
        SwitchSource();
        switchingSource.clip = clip;
        switchingSource.time = (float)timerEntryMusic;
        switchingSource.PlayScheduled(AudioSettings.dspTime + timerUntilPlayed);
        StartCoroutine(StopMusic(otherSource, (float)timerUntilPlayed));
    }

    void SwitchSource()
    {
        if (switchingSource == source1)
        {
            switchingSource = source2;
            otherSource = source1;
        }
        else
        {
            switchingSource = source1;
            otherSource = source2;
        }
    }
    IEnumerator StopMusic(AudioSource source, float timer)
    {
        yield return new WaitForSeconds(timer);
        source.Pause();
    }
}
