using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    #region Initialization
    [Range(1, 400)] public float bpm;
    [Range(0f, 3f)] public float fadeOutTime = 0.3f;
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
    private double _beatTime;
    public float BeatTime
    {
        get
        {
            return (float)_beatTime;
        }
    }
    public double audioTime { get { return AudioSettings.dspTime; } }
    private double timeBeforeNextBeat;
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
    public string currentSongName;
    [HideInInspector] public bool newMusicPlaying = false;
    [HideInInspector] public bool currentEnemyStatus;
    [HideInInspector] public int currentBarProgression = 1;
    [HideInInspector] public int currentSongProgression = 0;
    [HideInInspector] public int beatToSwitchTo = 4;
    [HideInInspector] public bool changingMusicZone = false;
    [HideInInspector] public string currentMusicSOName;

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
        audioDspTimeDelay = 0;
        useCameraBeatShake = true;
    }

    private void Update()
    {
        if (!musicStarted)
        {
            StartMusic();

            nextBeatStartTime = beatStartTimeOffset;
            nextOffBeatStartTime = nextBeatStartTime;
        }

        audioPlayTime = (float)AudioSettings.dspTime - songStartTime - audioDspTimeDelay;
        if (musicStarted && !GameManager.Instance.paused)
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

        if (OnBeat(GameManager.Instance.playerManager.playerOffBeated ,false))
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

    public bool OnBeat(bool offBeat, bool isAction)
    {
        bool onBeat = false;
        double beatTimeProgression;
        if(!offBeat || GameManager.Instance.playerManager.beatAndOffBeatAllowed)
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

        switchingSource.Play();

        songStartTime = audioTime;
    }

    public void PauseMusic()
    {
        if (switchingSource != null)
            switchingSource.Pause();
        pauseStartTime = audioPlayTime;
    }

    public void UnPauseMusic()
    {
        if (switchingSource != null)
            switchingSource.UnPause();

        audioDspTimeDelay += audioPlayTime - pauseStartTime;
    }

    private void MusicInit()
    {
        _beatTime = 60 / bpm;
        onBeatSingleFrame = false;
        beatActionUsed = false;
        newMusicPlaying = true;
    }

    public void LoadMusic(AudioClip clip)
    {
        switchingSource.clip = clip;
    }

    public void LoadMusic(AudioClip clip, float timerEntryMusic)
    {
        switchingSource.clip = clip;
        switchingSource.time = timerEntryMusic;
    }

    public void PlayMusicLoadedNextBeat()
    {
        switchingSource.PlayScheduled(timeBeforeNextBeat);
        StartCoroutine(FadeOutMusic(otherSource, (float)timeBeforeNextBeat, fadeOutTime));
        StartCoroutine(RefreshSongInfos((float)timeBeforeNextBeat));
        SwitchSource();
    }

    public void PlayMusicLoadedInSomeBeats(int numberOfBeatsUntilPlayed)
    {
        double timeUntilPlayed = timeBeforeNextBeat + numberOfBeatsUntilPlayed * BeatTime;
        switchingSource.PlayScheduled(timeUntilPlayed);
        StartCoroutine(FadeOutMusic(otherSource, (float)timeUntilPlayed, fadeOutTime));
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
    IEnumerator FadeOutMusic(AudioSource source, float timeUntilFadeOut, float fadeTime)
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
    }

    IEnumerator RefreshSongInfos(float timeUntilRefresh)
    {
        yield return new WaitForSeconds(timeUntilRefresh);
        newMusicPlaying = true;
        currentSongName = switchingSource.clip.name;
    }
}
