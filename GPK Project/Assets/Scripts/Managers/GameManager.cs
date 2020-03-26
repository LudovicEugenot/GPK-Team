using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    #endregion

    #region Initialization
    public BeatManager Beat; //majuscule parce que manager >>>> Oui mais non
    public GameObject player;
    public List<TransitionManager.TransitionHook> transitionHooks;
    [HideInInspector] public Blink blink;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public GameObject spriteRendererO;
    #endregion

    private void Start()
    {
        ProgressionManager.currentProgression = ProgressionManager.ProgressionState.Tutorial1;
        spriteRendererO = player.transform.GetChild(1).gameObject;
        blink = player.GetComponentInChildren<Blink>();
        playerManager = player.GetComponentInChildren<PlayerManager>();

        StartCoroutine(TransitionManager.Instance.ZoneInitialization(transitionHooks, spriteRendererO));
    }
}
