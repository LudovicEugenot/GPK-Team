using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Initialization
    [HideInInspector] public BeatManager Beat; //majuscule parce que manager >>>> Oui mais non
    public GameObject player;
    [HideInInspector] public Camera mainCamera;
    public List<TransitionManager.TransitionHook> transitionHooks;
    public GameObject hooksHolder;
    [HideInInspector] public List<HookState> zoneHooks;
    public GameObject enemiesHolder;
    [HideInInspector] public List<EnemyBase> zoneEnemies;
    public GameObject elementsHolder;
    [HideInInspector] public List<SwitchElement> zoneElements;
    [HideInInspector] public Blink blink;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public GameObject spriteRendererO;
    [HideInInspector] public ZoneHandler zoneHandler;
    [HideInInspector] public CameraHandler cameraHandler;
    [Space]
    public GameObject pausePanel;
    [HideInInspector] public bool paused;

    #endregion


    #region Singleton
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

    }
    #endregion

    private void Start()
    {
        FirstStart();
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

        paused = false;
        pausePanel.SetActive(false);
        Beat = BeatManager.Instance;
        mainCamera = Camera.main;
        cameraHandler = mainCamera.GetComponent<CameraHandler>();
        WorldManager.currentStoryStep = WorldManager.StoryStep.Tutorial1;
        spriteRendererO = player.transform.GetChild(1).gameObject;
        blink = player.GetComponentInChildren<Blink>();
        playerManager = player.GetComponentInChildren<PlayerManager>();
        StartCoroutine(TransitionManager.Instance.ZoneInitialization(zoneHooks, transitionHooks, GameManager.Instance.spriteRendererO, zoneEnemies.Count, zoneElements.Count));
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            // test whatever
        }

        if(Input.GetButtonDown("Cancel"))
        {
            if(paused)
            {
                UnPause();
            }
            else
            {
                Pause();
            }
        }
    }

    public void SaveGame()
    {
        UnPause();
        ZoneHandler.Instance.SaveZoneState();
        SaveSystem.SavePlayer(playerManager);
        SaveSystem.SaveWorld(zoneHandler);
        StartCoroutine(SavePreview());
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

    private IEnumerator AndBackToMainMenu()
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

    public void Pause()
    {
        pausePanel.SetActive(true);
        Beat.PauseMusic();
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
        paused = true;
    }

    public void UnPause()
    {
        pausePanel.SetActive(false);
        Beat.UnPauseMusic();
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;
        paused = false;
    }
}
