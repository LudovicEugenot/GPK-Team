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
            Destroy(this.gameObject);

    }
    #endregion

    private void Start()
    {
        FirstStart();
    }

    #region Initialization
    [HideInInspector] public BeatManager Beat; //majuscule parce que manager >>>> Oui mais non
    public GameObject player;
    public List<TransitionManager.TransitionHook> transitionHooks;
    public GameObject hooksHolder;
    [HideInInspector] public List<Hook> zoneHooks;
    public List<EnemyBase> zoneEnemies;
    public List<SwitchElement> zoneElements;
    [HideInInspector] public Blink blink;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public GameObject spriteRendererO;
    [HideInInspector] public ZoneHandler zoneHandler;
    #endregion

    void FirstStart()
    {
        zoneHooks = new List<Hook>();
        for(int i = 0; i < hooksHolder.transform.childCount; i++)
        {
            zoneHooks.Add(hooksHolder.transform.GetChild(i).GetComponent<Hook>());
        }
        Beat = BeatManager.Instance;
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
            //Test whatever you want ^^
        }
    }
}
