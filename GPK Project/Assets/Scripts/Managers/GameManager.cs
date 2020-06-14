using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    #region Initialization
    [HideInInspector] public BeatManager Beat; //majuscule parce que manager >>>> Oui mais non
    public GameObject player;
    [Space]
    public string zoneName;
    public GameObject zoneNameO;
    public int recolorHealthHealed;
    public int beatBeforeCombatStart;
    public float zoneNameDisplaySpeed;
    public float zoneNameDisplayTime;
    [HideInInspector] public Camera mainCamera;
    [Space]
    public List<TransitionManager.TransitionHook> transitionHooks;
    public GameObject hooksHolder;
    [HideInInspector] public List<HookState> zoneHooks;
    public GameObject enemiesHolder;
    [HideInInspector] public List<EnemyBase> zoneEnemies;
    public GameObject elementsHolder;
    public List<HeartContainerPart> heartContainers;
    public bool respawnEnnemiesIfZoneNotConverted;

    [HideInInspector] public List<SwitchElement> zoneElements;
    [HideInInspector] public Blink blink;
    [HideInInspector] public BlinkAttack attack;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public DialogueManager dialogueManager;
    [HideInInspector] public static AudioSource playerSource;
    [HideInInspector] public static RemoteSpeaker remoteSpeaker;
    [HideInInspector] public static Animator playerAnimator;
    [HideInInspector] public GameObject spriteRendererO;
    [HideInInspector] public ZoneHandler zoneHandler;
    [HideInInspector] public CameraHandler cameraHandler;
    [HideInInspector] public GameObject interfaceCanvas;
    [Space]
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider soundEffectsSlider;
    public Slider offsetSlider;
    public Text offsetValueText;
    public Toggle playtestToggle;
    public AudioMixer mixer;
    [HideInInspector] public bool paused;
    private bool optionsOpened;
    [Header("Sounds")]
    public AudioClip validationSound;
    public AudioClip backSound;

    private RectTransform zoneNameTransform;
    private Vector2 initialZoneNamePos;
    [HideInInspector] public bool usePlaytestRecord;
    [HideInInspector] public float masterVolume;
    [HideInInspector] public float musicVolume;
    [HideInInspector] public float soundEffectsVolume;

    private int beatRemainingBeforeCombatStart;
    private bool enemyPaused;

    #endregion


    #region Singleton
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(this.gameObject);

    }
    #endregion

    private void Start()
    {
        FirstStart();
        LoadPlayerPrefs();
    }

    void FirstStart()
    {
        zoneHooks = new List<HookState>();
        for (int i = 0; i < hooksHolder.transform.childCount; i++)
        {
            zoneHooks.Add(hooksHolder.transform.GetChild(i).GetComponent<Hook>().hookState);
        }

        zoneEnemies = new List<EnemyBase>();
        if(enemiesHolder != null)
        {
            for (int i = 0; i < enemiesHolder.transform.childCount; i++)
            {
                zoneEnemies.Add(enemiesHolder.transform.GetChild(i).GetComponentInChildren<EnemyBase>());
            }
        }

        ActiveValidEnemies();

        zoneElements = new List<SwitchElement>();
        if (elementsHolder != null)
        {
            for (int i = 0; i < elementsHolder.transform.childCount; i++)
            {
                zoneElements.Add(elementsHolder.transform.GetChild(i).GetComponentInChildren<SwitchElement>());
            }
        }

        beatRemainingBeforeCombatStart = beatBeforeCombatStart;
        zoneNameTransform = zoneNameO.GetComponent<RectTransform>();
        initialZoneNamePos = zoneNameTransform.anchoredPosition;
        zoneNameO.SetActive(false);
        paused = false;
        pausePanel.SetActive(false);
        Beat = BeatManager.Instance;
        mainCamera = Camera.main;
        playerSource = player.GetComponentInChildren<AudioSource>();
        cameraHandler = mainCamera.GetComponent<CameraHandler>();
        spriteRendererO = player.transform.GetChild(1).gameObject;
        blink = player.GetComponentInChildren<Blink>();
        attack = player.GetComponentInChildren<BlinkAttack>();
        playerManager = player.GetComponentInChildren<PlayerManager>();
        dialogueManager = player.GetComponentInChildren<DialogueManager>();
        remoteSpeaker = player.GetComponentInChildren<RemoteSpeaker>();
        playerAnimator = player.transform.GetChild(1).GetComponent<Animator>();
        interfaceCanvas = dialogueManager.dialogueBoxO.transform.parent.parent.gameObject;

        StartCoroutine(TransitionManager.Instance.ZoneInitialization(zoneHooks, transitionHooks, spriteRendererO, zoneEnemies.Count, zoneElements.Count, heartContainers.Count));
    }

    void LoadPlayerPrefs()
    {
        PlayerPrefData playerPref = SaveSystem.LoadPlayerPref();

        if (playerPref != null)
        {
            usePlaytestRecord = playerPref.usePlaytestRecord;
            playtestToggle.isOn = usePlaytestRecord;

            masterVolume = playerPref.masterVolume;
            masterSlider.value = masterVolume;

            musicVolume = playerPref.musicVolume;
            musicSlider.value = musicVolume;

            soundEffectsVolume = playerPref.soundEffectsVolume;
            soundEffectsSlider.value = soundEffectsVolume;

            Beat.timingThresholdOffset = playerPref.timingTresholdOffset;
            offsetSlider.value = playerPref.timingTresholdOffset;
            offsetValueText.text = (Mathf.Round(playerPref.timingTresholdOffset * 100) / 100).ToString() + " sec";
        }
        else
        {
            usePlaytestRecord = true;
            playtestToggle.isOn = true;
            masterVolume = Mathf.Log(0.5f, 1.1f);
            masterSlider.value = 0.5f;
            musicVolume = Mathf.Log(0.5f, 1.1f);
            musicSlider.value = 0.5f;
            soundEffectsVolume = Mathf.Log(0.5f, 1.1f);
            soundEffectsSlider.value = 0.5f;

            Beat.timingThresholdOffset = 0;
            offsetSlider.value = 0;
            offsetValueText.text = "0 sec";
        }

        RefreshVolumes();
    }

    void ActiveValidEnemies()
    {
        foreach(EnemyBase enemy in zoneEnemies)
        {
            if(enemy.firstStoryStepToAppear <= WorldManager.currentStoryStep && (enemy.lastStoryStepToAppear >= WorldManager.currentStoryStep || enemy.lastStoryStepToAppear == WorldManager.StoryStep.Tutorial))
            {
                enemy.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                enemy.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        UpdateCombatStart();

        if(Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.S))
        {
            WorldManager.GetWorldEvent(WorldManager.EventName.StringInstrumentRelived).occured = true;
        }
        if (Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.R))
        {
            WorldManager.GetWorldEvent(WorldManager.EventName.RythmInstrumentRelived).occured = true;
        }
        if (Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.V))
        {
            WorldManager.GetWorldEvent(WorldManager.EventName.VoiceInstrumentRelived).occured = true;
        }


        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log(WorldManager.currentStoryStep);
        }

        if (Input.GetButtonDown("Cancel"))
        {
            if(paused)
            {
                if(optionsOpened)
                {
                    CloseOptions();
                }
                else
                {
                    UnPause();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    private void UpdateCombatStart()
    {
        if (beatRemainingBeforeCombatStart > 0)
        {
            if(!enemyPaused)
                PauseEnemyBehaviour();
            if (Beat.onBeatSingleFrame)
            {
                beatRemainingBeforeCombatStart--;
            }
        }
        else if(beatRemainingBeforeCombatStart == 0 && enemyPaused)
        {
            beatRemainingBeforeCombatStart = -1;
            UnpauseEnemyBehaviour();
        }
    }

    #region Menu
    public void SaveGame()
    {
        if (usePlaytestRecord)
        {
            PlayTestRecorder.SaveCurrentZone();
            PlayTestRecorder.CreateTimingRecordFiles();
            PlayTestRecorder.CreateZoneRecordFile();
            PlayTestRecorder.ClearRecords();
        }
        UnPause();
        ZoneHandler.Instance.SaveZoneState();
        SaveSystem.SavePlayer(playerManager);
        SaveSystem.SaveWorld(zoneHandler);
        StartCoroutine(SavePreview());
        playerSource.PlayOneShot(validationSound);
    }

    public void SaveAndQuit()
    {
        SaveGame();
        StartCoroutine(AndQuit());
    }

    public void SaveAndBackToMainMenu()
    {
        //SaveGame();
        StartCoroutine(AndBackToMainMenu());
    }

    private IEnumerator AndQuit()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        Application.Quit();
    }

    public IEnumerator AndBackToMainMenu()
    {
        Destroy(Beat.gameObject);
        Destroy(zoneHandler.gameObject);
        yield return new WaitForSecondsRealtime(0.2f);
        UnPause();
        SceneManager.LoadScene(0);
    }

    private IEnumerator SavePreview()
    {
        yield return new WaitForEndOfFrame();
        Texture2D screenTexture = ScreenCapture.CaptureScreenshotAsTexture();
        yield return new WaitForEndOfFrame();
        SaveSystem.SavePreview(zoneHandler, screenTexture);
    }

    public void OpenOptions()
    {
        optionsOpened = true;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        playerSource.PlayOneShot(validationSound);
    }

    public void CloseOptions()
    {
        SaveSystem.SavePlayerPref(this);
        optionsOpened = false;
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        playerSource.time = 0.3f;
        playerSource.PlayOneShot(backSound);
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        if(dialogueManager.isTalking)
        {
            dialogueManager.dialogueBoxO.SetActive(false);
        }
        PauseTime();
        HideInterface();
        paused = true;
        playerSource.PlayOneShot(validationSound);
    }

    public void UnPause()
    {
        RefreshVolumes();
        SaveSystem.SavePlayerPref(this);
        pausePanel.SetActive(false);
        if (dialogueManager.isTalking)
        {
            dialogueManager.dialogueBoxO.SetActive(true);
        }
        UnPauseTime();
        ShowInterface();
        paused = false;
        playerSource.time = 0.3f;
        playerSource.PlayOneShot(backSound);
    }

    public void PauseTime()
    {
        Beat.PauseMusic();
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
    }

    public void UnPauseTime()
    {
        Beat.UnPauseMusic();
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;
    }

    private void RefreshVolumes()
    {
        mixer.SetFloat("Music", Mathf.Log(musicVolume, 1.1f));
        mixer.SetFloat("Master", Mathf.Log(masterVolume, 1.1f));
        mixer.SetFloat("SoundEffects", Mathf.Log(soundEffectsVolume, 1.1f));
    }

    public void NewMasterVolumeValue(float value)
    {
        masterVolume = value;
        RefreshVolumes();
    }
    public void NewMusicVolumeValue(float value)
    {
        musicVolume = value;
        RefreshVolumes();
    }
    public void NewSoundEffectsVolumeValue(float value)
    {
        soundEffectsVolume = value;
        RefreshVolumes();
    }
    public void NewOffsetValue()
    {
        Beat.timingThresholdOffset = offsetSlider.value;
        offsetValueText.text = (Mathf.Round(offsetSlider.value * 100) / 100).ToString() + " sec";
    }

    public void NewPlaytestValue(bool value)
    {
        usePlaytestRecord = value;
        playerSource.PlayOneShot(validationSound);
    }

    public void ResetToDefaultValue()
    {
        masterVolume = Mathf.Log(0.5f, 1.1f);
        masterSlider.value = 0.5f;

        musicVolume = Mathf.Log(0.5f, 1.1f);
        musicSlider.value = 0.5f;

        soundEffectsVolume = Mathf.Log(0.5f, 1.1f);
        soundEffectsSlider.value = 0.5f;

        Beat.timingThresholdOffset = 0;
        offsetSlider.value = 0;
        offsetValueText.text = "0 sec";


        usePlaytestRecord = true;
        playtestToggle.isOn = true;
        SaveSystem.SavePlayerPref(this);
    }
    #endregion

    public void PauseEnemyBehaviour()
    {
        enemyPaused = true;
        foreach(EnemyBase enemy in zoneEnemies)
        {
            enemy.enabled = false;
            
        }
    }

    public void UnpauseEnemyBehaviour()
    {
        enemyPaused = false;
        foreach (EnemyBase enemy in zoneEnemies)
        {
            enemy.enabled = true;
        }
    }

    public IEnumerator DisplayZoneName()
    {
        zoneNameO.GetComponentInChildren<Text>().text = zoneName;
        Vector2 hiddenPos = initialZoneNamePos + new Vector2(0, - initialZoneNamePos.y * 2 + 10);
        zoneNameTransform.anchoredPosition = hiddenPos;
        zoneNameO.SetActive(true);

        while (Vector2.Distance(initialZoneNamePos, zoneNameTransform.anchoredPosition) > 1f)
        {
            zoneNameTransform.anchoredPosition = Vector2.Lerp(zoneNameTransform.anchoredPosition, initialZoneNamePos, Time.deltaTime * zoneNameDisplaySpeed);
            yield return new WaitForEndOfFrame();
        }
        zoneNameTransform.anchoredPosition = initialZoneNamePos;
        yield return new WaitForSeconds(zoneNameDisplayTime);

        while (zoneNameTransform != null && Vector2.Distance(hiddenPos, zoneNameTransform.anchoredPosition) > 1f)
        {
            zoneNameTransform.anchoredPosition = Vector2.Lerp(zoneNameTransform.anchoredPosition, hiddenPos, Time.deltaTime * zoneNameDisplaySpeed);
            yield return new WaitForEndOfFrame();
        }

        if (zoneNameO != null)
        {
            zoneNameO.SetActive(false);
        }
    }

    public void HideInterface()
    {
        interfaceCanvas.SetActive(false);
    }
    public void ShowInterface()
    {
        interfaceCanvas.SetActive(true);
    }
}
