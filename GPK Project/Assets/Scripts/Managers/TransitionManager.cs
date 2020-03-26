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
        Instantiate(apparitionPrefab, startHook.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(timeBeforePlayerAppearence);
        currentPlayerRendererO.SetActive(true);
        GameManager.Instance.blink.transform.parent.position = startHook.transform.position;
    }


    public IEnumerator TransitionToConnectedZone(TransitionHook transitionHook)
    {
        newPlayerHp = GameManager.Instance.playerManager.currentHealth;
        Instantiate(disparitionPrefab, GameManager.Instance.blink.transform.position, Quaternion.identity);
        currentPlayerRendererO.SetActive(false);
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
