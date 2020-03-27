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
    private int newPlayerHp;

    public enum TransitionDirection { Up, Down, Right, Left };

    #region Singleton
    public static TransitionManager Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    #endregion


    private void Update()
    {
        if(Input.GetButtonDown("SpecialButton"))
        {
            foreach (TransitionHook transitionHook in currentTransitionHooks)
            {
                if ((Vector2)GameManager.Instance.blink.transform.position == (Vector2)transitionHook.hook.transform.position)
                {
                    StartCoroutine(TransitionToConnectedZone(transitionHook));
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


    public IEnumerator ZoneInitialization(List<TransitionHook> transitionHooks, GameObject playerRendererO)
    {
        if (startHook == null)
        {
            startHook = GameManager.Instance.blink.startHook;
        }

        blackScreen.SetActive(true);
        blackScreenMask.transform.localScale = Vector2.zero;
        currentTransitionHooks = transitionHooks;
        currentPlayerRendererO = playerRendererO;
        currentPlayerRendererO.SetActive(false);
        if (newPlayerHp != 0)
        {
            GameManager.Instance.playerManager.currentHealth = newPlayerHp;
            GameManager.Instance.playerManager.UpdateHealthBar();
        }
        foreach (TransitionHook transitionHook in currentTransitionHooks)
        {
            if (transitionHook.direction == currentStartDirection)
            {
                startHook = transitionHook.hook;
            }
        }

        GameManager.Instance.blink.transform.parent.position = startHook.transform.position;
        blackScreenMask.transform.position = currentPlayerRendererO.transform.position;
        float maskLerpProgression = 0;
        while(maskLerpProgression < 0.95f)
        {
            maskLerpProgression += (1 - maskLerpProgression) * transitionLerpSpeed * Time.deltaTime;
            blackScreenMask.transform.localScale = new Vector2(maskLerpProgression * maxMaskSize, maskLerpProgression * maxMaskSize);
            yield return new WaitForEndOfFrame();
        }
        Instantiate(apparitionPrefab, startHook.transform.position, Quaternion.identity);
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
