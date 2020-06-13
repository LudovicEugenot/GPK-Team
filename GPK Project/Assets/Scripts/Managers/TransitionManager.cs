using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public GameObject apparitionPrefab;
    public GameObject disparitionPrefab;
    public GameObject oneFootPrefab;
    public float timeBeforePlayerAppearence;
    public float timeBeforeZoneQuitting;
    [Space]
    public GameObject blackScreen;
    public GameObject blackScreenMask;
    [Range(0.1f,10)]
    public float transitionLerpSpeed;
    public float maxMaskSize;

    private List<TransitionHook> currentTransitionHooks;
    private TransitionDirection currentStartDirection;
    private GameObject currentPlayerRendererO;

    private Hook startHook;
    private Vector2 apparitionPos;
    [HideInInspector] public PlayerData previousPlayerData;
    private WorldData previousWorldData;
    [HideInInspector] public bool firstInit;
    private bool isTransitionning;
    private ZoneHandler zoneHandler;
    private float timeSpentInZone;
    [HideInInspector] public TransitionHook previousTransitionHook;

    public enum TransitionDirection { Up, Down, Right, Left , WIP};

    #region Singleton
    public static TransitionManager Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (Instance == null)
        {
            Instance = this;
            zoneHandler = GetComponent<ZoneHandler>();
            firstInit = true;
            previousPlayerData = null;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion


    private void Update()
    {
        CheckTransitionStart();

        if(GameManager.Instance != null)
        {
            timeSpentInZone += GameManager.Instance.paused ? 0 : Time.deltaTime;
        }
    }

    [System.Serializable]
    public class TransitionHook
    {
        public Hook hook;
        public bool isTemporary;
        public TransitionDirection direction;
        public int connectedSceneBuildIndex;
        public WorldManager.StoryStep storyStepRequired;
        public WorldManager.EventName eventRequired;
        public Talk blockedTalk;
        public int customCinematicTransitionSceneIndex;
    }

    private void CheckTransitionStart()
    {
        if(!firstInit && !GameManager.Instance.paused && !GameManager.Instance.dialogueManager.isTalking && !isTransitionning && PlayerManager.CanInteract())
        {
            foreach (TransitionHook transitionHook in currentTransitionHooks)
            {
                if (GameManager.Instance.blink.currentHook == transitionHook.hook && !transitionHook.isTemporary && transitionHook.connectedSceneBuildIndex >= 0)
                {
                    PlayerManager.DisplayIndicator();
                    if (Input.GetButtonDown("Blink") && PlayerManager.IsMouseNearPlayer())
                    {
                        if (transitionHook.connectedSceneBuildIndex < SceneManager.sceneCountInBuildSettings && transitionHook.direction != TransitionDirection.WIP)
                        {
                            zoneHandler.SaveZoneState();
                            if (zoneHandler.AllEnemiesConverted())
                            {
                                if ((int)WorldManager.currentStoryStep >= (int)transitionHook.storyStepRequired && (WorldManager.GetWorldEvent(transitionHook.eventRequired).occured || transitionHook.eventRequired == WorldManager.EventName.NullEvent))
                                {
                                    StartCoroutine(TransitionToConnectedZone(transitionHook));
                                }
                                else
                                {
                                    GameManager.Instance.dialogueManager.StartTalk(transitionHook.blockedTalk, GameManager.Instance.transform.position, 5.625f);
                                }
                            }
                            else
                            {
                                //feedback de non chagement de zone bloqué par ennemi !
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The transition hook : " + transitionHook.hook.gameObject.name + " leads to an unfinished or inexistant zone");
                        }
                    }
                }
            }
        }
    }

    public void StartSecretTransition(TransitionHook temporaryTransitionHook)
    {
        StartCoroutine(TransitionToConnectedZone(temporaryTransitionHook));
    }

    public IEnumerator ZoneInitialization(List<HookState> zoneHooks, List<TransitionHook> transitionHooks, GameObject playerRendererO, int enemyNumber, int elementNumber, int heartContainerNumber)
    {
        ZoneHandler.Zone potentialZone = null;
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        foreach (ZoneHandler.Zone zone in zoneHandler.zones)
        {
            if(zone.buildIndex == currentBuildIndex)
            {
                potentialZone = zone;
            }
        }

        if(potentialZone == null)
        {
            potentialZone = new ZoneHandler.Zone(currentBuildIndex, GameManager.Instance.zoneName, zoneHooks, enemyNumber, elementNumber, heartContainerNumber);
            zoneHandler.zones.Add(potentialZone);
        }

        blackScreen.SetActive(true);
        blackScreenMask.transform.localScale = Vector2.zero;
        currentTransitionHooks = transitionHooks;
        currentPlayerRendererO = playerRendererO;
        currentPlayerRendererO.SetActive(false);

        yield return new WaitForEndOfFrame();

        zoneHandler.InitializeZone(potentialZone);

        timeSpentInZone = 0;

        if (startHook == null)
        {
            startHook = GameManager.Instance.blink.startHook;
        }


        if (previousPlayerData != null)
        {
            GameManager.Instance.playerManager.maxhealthPoint = previousPlayerData.maxHealthPoint;
            GameManager.Instance.playerManager.currentHealth = previousPlayerData.health;
            GameManager.Instance.playerManager.ownSpeaker = previousPlayerData.ownSpeaker;
            GameManager.Instance.playerManager.heartContainerOwned = previousPlayerData.heartContainerOwned;
        }
        else
        {
            GameManager.Instance.playerManager.currentHealth = GameManager.Instance.playerManager.maxhealthPoint * 2;
        }

        GameManager.Instance.playerManager.InitializeHealthBar();

        if (!firstInit)
        {
            foreach (TransitionHook transitionHook in currentTransitionHooks)
            {
                if (transitionHook.direction == currentStartDirection)
                {
                    startHook = transitionHook.hook;
                }
            }
            GameManager.Instance.playerManager.transform.parent.position = startHook.transform.position;
            apparitionPos = startHook.transform.position;
        }
        else
        {
            if(previousPlayerData != null)
            {
                GameManager.Instance.playerManager.transform.parent.position = new Vector2(previousPlayerData.position[0], previousPlayerData.position[1]);
                apparitionPos = new Vector2(previousPlayerData.position[0], previousPlayerData.position[1]);
            }
            else
            {
                GameManager.Instance.playerManager.transform.parent.position = startHook.transform.position;
                apparitionPos = startHook.transform.position;
            }
        }
        GameManager.Instance.blink.currentHook = startHook;

        PlayTestRecorder.RefreshCurrentZone(zoneHandler.currentZone.name);

        blackScreenMask.transform.position = currentPlayerRendererO.transform.position;
        float maskLerpProgression = 0;
        while(maskLerpProgression < 0.95f)
        {
            maskLerpProgression += (1 - maskLerpProgression) * transitionLerpSpeed * Time.deltaTime;
            blackScreenMask.transform.localScale = new Vector2(maskLerpProgression * maxMaskSize, maskLerpProgression * maxMaskSize);
            yield return new WaitForEndOfFrame();
        }
        Instantiate(apparitionPrefab, !firstInit ? startHook.transform.position + Vector3.up * GameManager.Instance.player.transform.GetChild(1).transform.localPosition.y : (Vector3)apparitionPos, Quaternion.identity);
        firstInit = false;
        yield return new WaitForSeconds(timeBeforePlayerAppearence);
        isTransitionning = false;
        StartCoroutine(startHook.BlinkReaction(true));
        currentPlayerRendererO.SetActive(true);
        GameManager.Instance.playerManager.animSynchronizer.Synchronize();
        blackScreen.SetActive(false);

        StartCoroutine(GameManager.Instance.DisplayZoneName());
    }

    public IEnumerator TransitionToConnectedZone(TransitionHook transitionHook)
    {
        GameObject oneFoot = null;
        if(transitionHook.customCinematicTransitionSceneIndex != 0)
        {
            currentPlayerRendererO.SetActive(false);
            oneFoot = Instantiate(oneFootPrefab, GameManager.Instance.player.transform.position + Vector3.up * GameManager.Instance.player.transform.GetChild(1).transform.localPosition.y, Quaternion.identity);
            Destroy(transitionHook.hook.gameObject);
            yield return new WaitForSeconds(2);
        }

        isTransitionning = true;
        blackScreen.SetActive(true);
        blackScreenMask.transform.localScale = Vector2.one * maxMaskSize;
        blackScreenMask.transform.position = currentPlayerRendererO.transform.position;
        GameManager.Instance.PauseEnemyBehaviour();

        previousPlayerData = new PlayerData(GameManager.Instance.playerManager);
        zoneHandler.SaveZoneState();
        previousWorldData = new WorldData(zoneHandler);
        PlayTestRecorder.currentZoneRecord.timeSpent += timeSpentInZone;
        PlayTestRecorder.SaveCurrentZone();


        float maskLerpProgression = 0;
        while (maskLerpProgression < 0.92f)
        {
            maskLerpProgression += (1 - maskLerpProgression) * transitionLerpSpeed * Time.deltaTime;
            blackScreenMask.transform.localScale = new Vector2((1 - maskLerpProgression) * maxMaskSize, (1 - maskLerpProgression) * maxMaskSize);
            if (transitionHook.customCinematicTransitionSceneIndex != 0)
            {
                oneFoot.transform.position += Vector3.down * Time.deltaTime * 10;
            }
            yield return new WaitForEndOfFrame();
        }

        if(transitionHook.customCinematicTransitionSceneIndex == 0)
        {
            currentPlayerRendererO.SetActive(false);
            Instantiate(disparitionPrefab, GameManager.Instance.blink.transform.position + Vector3.up * GameManager.Instance.player.transform.GetChild(1).transform.localPosition.y, Quaternion.identity);
        }


        GameManager.Instance.StopAllCoroutines();

        yield return new WaitForSeconds(timeBeforeZoneQuitting);
        GameManager.playerSource.PlayOneShot(GameManager.Instance.blink.transitionBlinkSound);

        switch (transitionHook.direction)
        {
            case TransitionDirection.Down:
                currentStartDirection = TransitionDirection.Up;
                break;

            case TransitionDirection.Up:
                currentStartDirection = TransitionDirection.Down;
                break;

            case TransitionDirection.Right:
                currentStartDirection = TransitionDirection.Left;
                break;

            case TransitionDirection.Left:
                currentStartDirection = TransitionDirection.Right;
                break;
        }
        zoneHandler.zoneInitialized = false;
        previousTransitionHook = transitionHook;
        SceneManager.LoadScene(transitionHook.customCinematicTransitionSceneIndex == 0 ? transitionHook.connectedSceneBuildIndex : transitionHook.customCinematicTransitionSceneIndex);
    }

    public IEnumerator Respawn()
    {
        GameManager.Instance.playerManager.isInControl = false;
        GameManager.Instance.playerManager.Heal(500, false);

        previousPlayerData = new PlayerData(GameManager.Instance.playerManager);

        blackScreen.SetActive(true);
        blackScreenMask.transform.localScale = Vector2.one * maxMaskSize;
        blackScreenMask.transform.position = currentPlayerRendererO.transform.position;

        PlayTestRecorder.currentZoneRecord.timeSpent += timeSpentInZone;

        float maskLerpProgression = 0;
        while (maskLerpProgression < 0.92f)
        {
            maskLerpProgression += (1 - maskLerpProgression) * transitionLerpSpeed * Time.deltaTime;
            blackScreenMask.transform.localScale = new Vector2((1 - maskLerpProgression) * maxMaskSize, (1 - maskLerpProgression) * maxMaskSize);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(timeBeforeZoneQuitting);

        switch (currentStartDirection)
        {
            case TransitionDirection.Down:
                currentStartDirection = TransitionDirection.Up;
                break;

            case TransitionDirection.Up:
                currentStartDirection = TransitionDirection.Down;
                break;

            case TransitionDirection.Right:
                currentStartDirection = TransitionDirection.Left;
                break;

            case TransitionDirection.Left:
                currentStartDirection = TransitionDirection.Right;
                break;
        }

        BeatManager.Instance.UnMuteMusic();
        if (previousWorldData != null)
        {
            zoneHandler.zones = previousWorldData.worldZones;
            WorldManager.allWorldEvents = previousWorldData.worldEvents;
            WorldManager.currentStoryStep = previousWorldData.storyStep;
            yield return new WaitForEndOfFrame();
            SceneManager.LoadScene(previousWorldData.savedZoneBuildIndex);
        }
        else
        {
            yield return new WaitForEndOfFrame();
            SceneManager.LoadScene(zoneHandler.currentZone.buildIndex);
        }
    }
}
