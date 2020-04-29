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
    [HideInInspector] public Vector2 savePos;
    [HideInInspector] public int newPlayerHp;
    [HideInInspector] public bool firstInit;
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
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion


    private void Update()
    {
        if(Input.GetButtonDown("Interact"))
        {
            foreach (TransitionHook transitionHook in currentTransitionHooks)
            {
                if (GameManager.Instance.blink.currentHook == transitionHook.hook)
                {
                    if(transitionHook.connectedSceneBuildIndex < SceneManager.sceneCountInBuildSettings && transitionHook.direction != TransitionDirection.WIP)
                    {
                        StartCoroutine(TransitionToConnectedZone(transitionHook));
                    }
                    else
                    {
                        Debug.LogWarning("The transition hook : " + transitionHook.hook.gameObject.name + " leads to an unfinished or inexistant zone");
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class TransitionHook
    {
        public Hook hook;
        public TransitionDirection direction;
        public int connectedSceneBuildIndex;
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
            potentialZone = new ZoneHandler.Zone(currentBuildIndex, SceneManager.GetActiveScene().name, zoneHooks, enemyNumber, elementNumber);
            zoneHandler.zones.Add(potentialZone);
        }

        blackScreen.SetActive(true);
        blackScreenMask.transform.localScale = Vector2.zero;
        currentTransitionHooks = transitionHooks;
        currentPlayerRendererO = playerRendererO;
        currentPlayerRendererO.SetActive(false);

        yield return new WaitForEndOfFrame();

        zoneHandler.InitializeZone(potentialZone);

        if (newPlayerHp != 0)
        {
            GameManager.Instance.playerManager.currentHealth = newPlayerHp;
            if(!GameManager.Instance.playerManager.InitializeHealthBar())
            {
                GameManager.Instance.playerManager.UpdateHealthBar();
            }
        }


        if (startHook == null)
        {
            startHook = GameManager.Instance.blink.startHook;
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
        }
        else
        {
            GameManager.Instance.playerManager.transform.parent.position = savePos;
            if(savePos == Vector2.zero)
            {
                GameManager.Instance.playerManager.transform.parent.position = startHook.transform.position;
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
        Instantiate(apparitionPrefab, !firstInit ? startHook.transform.position : (Vector3)savePos, Quaternion.identity);
        firstInit = false;
        yield return new WaitForSeconds(timeBeforePlayerAppearence);
        currentPlayerRendererO.SetActive(true);
        blackScreen.SetActive(false);
    }


    public IEnumerator TransitionToConnectedZone(TransitionHook transitionHook)
    {
        blackScreen.SetActive(true);
        blackScreenMask.transform.localScale = Vector2.one * maxMaskSize;
        blackScreenMask.transform.position = currentPlayerRendererO.transform.position;
        newPlayerHp = GameManager.Instance.playerManager.currentHealth;
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
