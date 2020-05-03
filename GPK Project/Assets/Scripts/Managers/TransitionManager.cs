using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public GameObject apparitionPrefab;
    public GameObject disparitionPrefab;
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
    [HideInInspector] public PlayerData newPlayerData;
    [HideInInspector] public bool firstInit;
    private bool isTransitionning;
    private ZoneHandler zoneHandler;

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
            newPlayerData = null;
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
    }

    [System.Serializable]
    public class TransitionHook
    {
        public Hook hook;
        public bool isTemporary;
        public TransitionDirection direction;
        public int connectedSceneBuildIndex;
    }

    private void CheckTransitionStart()
    {
        if (!firstInit && Input.GetButtonDown("Blink") && !GameManager.Instance.blink.IsSelecting() && !GameManager.Instance.dialogueManager.isTalking && !isTransitionning)
        {
            foreach (TransitionHook transitionHook in currentTransitionHooks)
            {
                if (GameManager.Instance.blink.currentHook == transitionHook.hook && !transitionHook.isTemporary && transitionHook.connectedSceneBuildIndex >= 0)
                {
                    if (transitionHook.connectedSceneBuildIndex < SceneManager.sceneCountInBuildSettings && transitionHook.direction != TransitionDirection.WIP)
                    {
                        zoneHandler.SaveZoneState();
                        if (zoneHandler.AllEnemiesConverted())
                        {
                            StartCoroutine(TransitionToConnectedZone(transitionHook));
                        }
                        else
                        {
                            //feedback de non chagement de zone
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

    public void StartSecretTransition(TransitionHook temporaryTransitionHook)
    {
        StartCoroutine(TransitionToConnectedZone(temporaryTransitionHook));
    }


    public IEnumerator ZoneInitialization(List<HookState> zoneHooks, List<TransitionHook> transitionHooks, GameObject playerRendererO, int enemyNumber, int elementNumber)
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
            potentialZone = new ZoneHandler.Zone(currentBuildIndex, GameManager.Instance.zoneName, zoneHooks, enemyNumber, elementNumber);
            zoneHandler.zones.Add(potentialZone);
        }

        blackScreen.SetActive(true);
        blackScreenMask.transform.localScale = Vector2.zero;
        currentTransitionHooks = transitionHooks;
        currentPlayerRendererO = playerRendererO;
        currentPlayerRendererO.SetActive(false);

        yield return new WaitForEndOfFrame();

        zoneHandler.InitializeZone(potentialZone);



        if (startHook == null)
        {
            startHook = GameManager.Instance.blink.startHook;
        }


        if (newPlayerData != null)
        {
            GameManager.Instance.playerManager.maxhealthPoint = newPlayerData.maxHealthPoint;
            GameManager.Instance.playerManager.currentHealth = newPlayerData.health;
            GameManager.Instance.playerManager.ownSpeaker = newPlayerData.ownSpeaker;
        }
        else
        {
            GameManager.Instance.playerManager.currentHealth = GameManager.Instance.playerManager.maxhealthPoint * 2;
        }

        if (!GameManager.Instance.playerManager.InitializeHealthBar())
        {
            GameManager.Instance.playerManager.UpdateHealthBar();
        }

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
            if(newPlayerData != null)
            {
                GameManager.Instance.playerManager.transform.parent.position = new Vector2(newPlayerData.position[0], newPlayerData.position[1]);
                apparitionPos = new Vector2(newPlayerData.position[0], newPlayerData.position[1]);
            }
            else
            {
                GameManager.Instance.playerManager.transform.parent.position = startHook.transform.position;
                apparitionPos = startHook.transform.position;
            }
        }
        GameManager.Instance.blink.currentHook = startHook;

        blackScreenMask.transform.position = currentPlayerRendererO.transform.position;
        float maskLerpProgression = 0;
        while(maskLerpProgression < 0.95f)
        {
            maskLerpProgression += (1 - maskLerpProgression) * transitionLerpSpeed * Time.deltaTime;
            blackScreenMask.transform.localScale = new Vector2(maskLerpProgression * maxMaskSize, maskLerpProgression * maxMaskSize);
            yield return new WaitForEndOfFrame();
        }
        Instantiate(apparitionPrefab, !firstInit ? startHook.transform.position : (Vector3)apparitionPos, Quaternion.identity);
        firstInit = false;
        yield return new WaitForSeconds(timeBeforePlayerAppearence);
        isTransitionning = false;
        StartCoroutine(startHook.BlinkReaction(true));
        currentPlayerRendererO.SetActive(true);
        blackScreen.SetActive(false);

        StartCoroutine(GameManager.Instance.DisplayZoneName());
    }


    public IEnumerator TransitionToConnectedZone(TransitionHook transitionHook)
    {
        isTransitionning = true;
        blackScreen.SetActive(true);
        blackScreenMask.transform.localScale = Vector2.one * maxMaskSize;
        blackScreenMask.transform.position = currentPlayerRendererO.transform.position;
        newPlayerData = new PlayerData(GameManager.Instance.playerManager);
        zoneHandler.SaveZoneState();

        float maskLerpProgression = 0;
        while (maskLerpProgression < 0.92f)
        {
            maskLerpProgression += (1 - maskLerpProgression) * transitionLerpSpeed * Time.deltaTime;
            blackScreenMask.transform.localScale = new Vector2((1 - maskLerpProgression) * maxMaskSize, (1 - maskLerpProgression) * maxMaskSize);
            yield return new WaitForEndOfFrame();
        }
        currentPlayerRendererO.SetActive(false);
        Instantiate(disparitionPrefab, GameManager.Instance.blink.transform.position, Quaternion.identity);

        GameManager.Instance.StopAllCoroutines();

        yield return new WaitForSeconds(timeBeforeZoneQuitting);

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

            SceneManager.LoadScene(transitionHook.connectedSceneBuildIndex);
    }
}
