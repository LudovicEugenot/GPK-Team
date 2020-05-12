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
    [Space]
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider soundEffectsSlider;
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
    private float masterVolume;
    private float musicVolume;
    private float soundEffectsVolume;

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


        zoneElements = new List<SwitchElement>();
        if (elementsHolder != null)
        {
            for (int i = 0; i < elementsHolder.transform.childCount; i++)
            {
                zoneElements.Add(elementsHolder.transform.GetChild(i).GetComponentInChildren<SwitchElement>());
            }
        }

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

        StartCoroutine(TransitionManager.Instance.ZoneInitialization(zoneHooks, transitionHooks, spriteRendererO, zoneEnemies.Count, zoneElements.Count, heartContainers.Count));
    }

    void LoadPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("UsePlaytestRecord"))
        {
            usePlaytestRecord = PlayerPrefs.GetInt("UsePlaytestRecord") == 1 ? true : false;
            playtestToggle.isOn = usePlaytestRecord;
        }
        else
        {
            usePlaytestRecord = true;
            playtestToggle.isOn = true;
        }

        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterVolume = Mathf.Log(PlayerPrefs.GetFloat("MasterVolume"),1.1f);
            masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        }
        else
        {
            masterVolume = Mathf.Log(0.5f, 1.1f);
            masterSlider.value = 0.5f;
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolume = Mathf.Log(PlayerPrefs.GetFloat("MusicVolume"),1.1f);
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        }
        else
        {
            musicVolume = Mathf.Log(0.5f, 1.1f);
            musicSlider.value = 0.5f;
        }


        if (PlayerPrefs.HasKey("SoundEffectsVolume"))
        {
            soundEffectsVolume = Mathf.Log(PlayerPrefs.GetFloat("SoundEffectsVolume"),1.1f);
            soundEffectsSlider.value = PlayerPrefs.GetFloat("SoundEffectsVolume");
        }
        else
        {
            soundEffectsVolume = Mathf.Log(0.5f, 1.1f);
            soundEffectsSlider.value = 0.5f;
        }
        RefreshVolumes();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            //do whatever
        }

        if(Input.GetButtonDown("Cancel"))
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

    #region Menu
    public void SaveGame()
    {
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
        SaveGame();
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
        optionsOpened = false;
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        playerSource.time = 0.3f;
        playerSource.PlayOneShot(backSound);
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        Beat.PauseMusic();
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
        paused = true;
        playerSource.PlayOneShot(validationSound);
    }

    public void UnPause()
    {
        RefreshVolumes();
        PlayerPrefs.Save();
        pausePanel.SetActive(false);
        Beat.UnPauseMusic();
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;
        paused = false;
        playerSource.time = 0.3f;
        playerSource.PlayOneShot(backSound);
    }

    private void RefreshVolumes()
    {
        mixer.SetFloat("Music", musicVolume);
        mixer.SetFloat("Master", masterVolume);
        mixer.SetFloat("SoundEffects", soundEffectsVolume);
    }

    public void NewMasterVolumeValue(float value)
    {
        masterVolume = Mathf.Log(value, 1.1f);
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }
    public void NewMusicVolumeValue(float value)
    {
        musicVolume = Mathf.Log(value, 1.1f);
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }
    public void NewSoundEffectsVolumeValue(float value)
    {
        soundEffectsVolume = Mathf.Log(value, 1.1f);
        PlayerPrefs.SetFloat("SoundEffectsVolume", value);
        PlayerPrefs.Save();
    }

    public void NewPlaytestValue(bool value)
    {
        PlayerPrefs.SetInt("UsePlaytestRecord", value ? 1 : 0);
        usePlaytestRecord = value;
        PlayerPrefs.Save();
        playerSource.PlayOneShot(validationSound);
    }
    #endregion


    public void PauseEnemyBehaviour()
    {
        foreach(EnemyBase enemy in zoneEnemies)
        {
            enemy.enabled = false;
        }
    }

    public void UnpauseEnemyBehaviour()
    {
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
}
