using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    #region Initialization
    public int timingLinePointNumber;
    [HideInInspector] [Range(1, 400)] public float bpm;
    [Range(0f, 3f)] public float fadeOutTime = 0.3f;
    [Tooltip("L'intervalle de temps dont le joueur dispose pour effectuer son action et être en rythme.")]
    [HideInInspector] [Range(0f, 1f)] public float timingThreshold = 0.2f;
    [HideInInspector] public float timingThresholdOffset;
    [HideInInspector] [Range(0f, 1f)] public float beatStartTimeOffset;
    [HideInInspector] [Range(0f, 1f)] public float minTimeForOnBeatValidation;

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
    public double audioTime { get { return AudioSettings.dspTime; } }
    [HideInInspector] public double timeBeforeNextBeat;
    private double timeBeforeNextOffBeat;
    [HideInInspector] public float currentBeatProgression;
    [HideInInspector] public bool useCameraBeatShake;
    private double nextBeatStartTime;
    private double nextOffBeatStartTime;
    private double songStartTime;
    private double audioDspTimeDelay;
    private double pauseStartTime;
    private double audioPlayTime;
    private bool beatActionUsed;
    private double lastActionTime;
    private bool actOnBeatPossible;

    private AudioSource switchingSource;
    private AudioSource otherSource;
    private AudioSource source1;
    private AudioSource source2;
    private AudioSource breakSource;
    public string currentSongName;
    [HideInInspector] public bool newMusicPlaying = false;
    [HideInInspector] public bool currentEnemyStatus;
    [HideInInspector] public int currentBarProgression = 1;
    [HideInInspector] public int currentSongProgression = 0;
    [HideInInspector] public int beatToSwitchTo = 4;
    [HideInInspector] public bool changingMusicZone = false;
    [HideInInspector] public string currentMusicSOName;
    [HideInInspector] public bool breakLoaded = false;
    [HideInInspector] public bool playingBreak = false;

    private float initialCameraSize;

    private LineRenderer timingLine;
    private Vector3[] timingLinePointPos;
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
            breakSource = sources[2];
            switchingSource = source1;
            otherSource = source2;
        }
        else
            Destroy(this.gameObject);
    }
    #endregion

    void Start()
    {
        musicStarted = false;
        //MusicInit();
        initialCameraSize = Camera.main.orthographicSize;
        audioDspTimeDelay = 0;
        useCameraBeatShake = true;
    }

    private void Update()
    {
        /*if (!musicStarted)
        {
            StartMusic();

            nextBeatStartTime = beatStartTimeOffset;
            nextOffBeatStartTime = nextBeatStartTime;
        }*/

        audioPlayTime = (float)AudioSettings.dspTime - songStartTime - audioDspTimeDelay;
        if (musicStarted && !GameManager.Instance.paused)
        {
            TimeCycle();
        }

        if (onBeatNextFrame && !beatActionUsed && !GameManager.Instance.dialogueManager.isTalking)
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

        if (OnBeat(GameManager.Instance.playerManager.playerOffBeated ,false, ""))
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

        if (nextBeatStartTime < audioPlayTime)
        {
            nextBeatStartTime += _beatTime;
            onBeatSingleFrame = true;
            if (!GameManager.Instance.playerManager.playerOffBeated)
            {
                if (cameraBeatEffectAmplitude != 0 && useCameraBeatShake)
                {
                    StartCoroutine(BeatEffect(1.0f));
                }
            }
            if(GameManager.Instance.playerManager.playerOffBeated || GameManager.Instance.playerManager.beatAndOffBeatAllowed)
            {
                beatActionUsed = false;
            }
        }

        if (nextOffBeatStartTime < audioPlayTime - _beatTime / 2)
        {
            nextOffBeatStartTime += _beatTime;
            if(GameManager.Instance.playerManager.playerOffBeated)
            {
                if (cameraBeatEffectAmplitude != 0 && useCameraBeatShake)
                {
                    StartCoroutine(BeatEffect(1.0f));
                }
            }
            if (!GameManager.Instance.playerManager.playerOffBeated || GameManager.Instance.playerManager.beatAndOffBeatAllowed)
            {
                beatActionUsed = false;
            }
        }

        timeBeforeNextBeat = nextBeatStartTime - audioPlayTime;
        timeBeforeNextOffBeat = nextOffBeatStartTime - audioPlayTime + _beatTime * 0.5f;
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

    public bool OnBeat(bool offBeat, bool isAction, string actionName)
    {
        bool onBeat = false;
        double beatTimeProgression = _beatTime - timeBeforeNextBeat;
        if (!offBeat || GameManager.Instance.playerManager.beatAndOffBeatAllowed)
        {
            beatTimeProgression = _beatTime - timeBeforeNextBeat;
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
        }

        if(offBeat || GameManager.Instance.playerManager.beatAndOffBeatAllowed)
        {
            beatTimeProgression = _beatTime - timeBeforeNextOffBeat;

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
                    if (timeBeforeNextOffBeat < (timingThreshold / 2) - timingThresholdOffset && timeBeforeNextOffBeat> -timingThresholdOffset - (timingThreshold / 2))
                    {
                        onBeat = true;
                    }
                }
                else
                {
                    if (beatTimeProgression < (timingThreshold / 2) + timingThresholdOffset || timeBeforeNextOffBeat< (timingThreshold / 2) - timingThresholdOffset)
                    {
                        onBeat = true;
                    }
                }
            }
        }

        if(isAction)
        {
            PlayTestRecorder.records.allRecords.Add(GetNewTimingRecord(onBeat, beatTimeProgression, actionName));
        }

        return onBeat;
    }

    public PlayTestRecorder.TimingRecord GetNewTimingRecord(bool onBeat, double beatTimeProgression, string actionName)
    {
        PlayTestRecorder.TimingRecord record = new PlayTestRecorder.TimingRecord();
        double offset = 0;
        if(currentBeatProgression > 0.5f)
        {
            offset = -timeBeforeNextBeat;
        }
        else
        {
            offset = beatTimeProgression;
        }
        record.playerOffsetWithTiming = offset;
        record.actionName = actionName;
        record.inCombat = !ZoneHandler.Instance.AllEnemiesConverted();
        record.zone = ZoneHandler.Instance.currentZone.name;
        record.musicBpm = bpm;
        record.onBeat = onBeat;
        return record;
    }

    public bool CanAct()
    {
        bool used = beatActionUsed;
        if (!beatActionUsed)
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

    public void StartNewMusic()
    {
        musicStarted = true;
        _beatTime = 60 / bpm;
        audioDspTimeDelay = 0;
        songStartTime = audioTime;
        nextBeatStartTime = beatStartTimeOffset;
        nextOffBeatStartTime = nextBeatStartTime;
        onBeatSingleFrame = false;
        beatActionUsed = false;
        switchingSource.Play();
    }

    public void PauseMusic()
    {
        if (otherSource != null)
        {
            switchingSource.Pause();
            otherSource.Pause();
        }
        pauseStartTime = audioPlayTime;
    }

    public void UnPauseMusic()
    {
        if (otherSource != null)
        {
            otherSource.UnPause();
            switchingSource.UnPause();
        }

        audioDspTimeDelay += audioPlayTime - pauseStartTime;
    }

    private void MusicInit()
    {
        _beatTime = 60 / bpm;
        onBeatSingleFrame = false;
        beatActionUsed = false;
    }

    public void LoadMusic(AudioClip clip)
    {
        switchingSource.clip = clip;
    }

    public void LoadMusic(AudioClip clip, float timerEntryMusic)
    {
        switchingSource.clip = clip;
        switchingSource.time = timerEntryMusic;
        /*beatStartTimeOffset = timerEntryMusic;
        nextBeatStartTime += timerEntryMusic;
        nextOffBeatStartTime += timerEntryMusic;*/
    }

    public void LoadBreak(AudioClip clip, float timerEntryMusic)
    {
        breakSource.clip = clip;
        breakSource.time = timerEntryMusic;
        breakLoaded = true;
    }

    public void PlayMusicLoadedNextBeat()
    {
        switchingSource.PlayScheduled(timeBeforeNextBeat);
        if (playingBreak)
            StartCoroutine(FadeOutMusic(breakSource, (float)timeBeforeNextBeat, fadeOutTime, true));
        else
            StartCoroutine(FadeOutMusic(otherSource, (float)timeBeforeNextBeat, fadeOutTime, false));
        StartCoroutine(RefreshSongInfos((float)timeBeforeNextBeat));
        SwitchSource();
    }

    public void PlayBreakNextBeat()
    {
        breakSource.PlayScheduled(timeBeforeNextBeat);
        StartCoroutine(FadeOutMusic(otherSource, (float)timeBeforeNextBeat, fadeOutTime, false));
        StartCoroutine(RefreshSongInfos((float)timeBeforeNextBeat));
        StartCoroutine(PlayingBreak((float)timeBeforeNextBeat));
    }

    public void PlayMusicLoadedInSomeBeats(int numberOfBeatsUntilPlayed)
    {
        double timeUntilPlayed = timeBeforeNextBeat + numberOfBeatsUntilPlayed * BeatTime;
        switchingSource.PlayScheduled(timeUntilPlayed);
        if (playingBreak)
            StartCoroutine(FadeOutMusic(breakSource, (float)timeUntilPlayed, fadeOutTime, true));
        else
            StartCoroutine(FadeOutMusic(otherSource, (float)timeUntilPlayed, fadeOutTime, false));
        StartCoroutine(RefreshSongInfos((float)timeUntilPlayed));
        SwitchSource();
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

    IEnumerator FadeOutMusic(AudioSource source, float timeUntilFadeOut, float fadeTime, bool fadedOutMusicIsBreak)
    {
        yield return new WaitForSeconds(timeUntilFadeOut);
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        source.Stop();
        source.volume = startVolume;

        if (fadedOutMusicIsBreak)
        {
            breakLoaded = false;
            playingBreak = false;
        }
    }

    IEnumerator RefreshSongInfos(float timeUntilRefresh)
    {
        yield return new WaitForSeconds(timeUntilRefresh);
        newMusicPlaying = true;
        currentSongName = otherSource.clip.name;
    }

    IEnumerator PlayingBreak(float timeUntilBreakStarted)
    {
        yield return new WaitForSeconds(timeUntilBreakStarted);
        playingBreak = true;
    }
}
