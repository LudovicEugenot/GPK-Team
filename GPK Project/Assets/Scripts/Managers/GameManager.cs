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
    public GameObject enemiesHolder;
    [HideInInspector] public List<EnemyBase> zoneEnemies;
    public List<SwitchElement> zoneElements;
    [HideInInspector] public Blink blink;
    [HideInInspector] public PlayerManager playerManager;
    [HideInInspector] public GameObject spriteRendererO;
    [HideInInspector] public ZoneHandler zoneHandler;

    [Space]
    public string alternateSavePath;
    #endregion

    void FirstStart()
    {
        zoneHooks = new List<Hook>();
        for(int i = 0; i < hooksHolder.transform.childCount; i++)
        {
            zoneHooks.Add(hooksHolder.transform.GetChild(i).GetComponent<Hook>());
        }

        zoneEnemies = new List<EnemyBase>();
        if(enemiesHolder != null)
        {
            for (int i = 0; i < enemiesHolder.transform.childCount; i++)
            {
                zoneEnemies.Add(enemiesHolder.transform.GetChild(i).GetComponentInChildren<EnemyBase>());
            }
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
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            //Test whatever you want ^^
            LoadGame();
        }
    }

    public void SaveGame()
    {
        SaveSystem.SavePlayer(playerManager, alternateSavePath);
    }

    public void LoadGame()
    {
        LoadPlayer();
    }

    private void LoadPlayer()
    {
        PlayerData player = SaveSystem.LoadPlayer(alternateSavePath);
        if(player != null)
        {
            playerManager.maxhealthPoint = player.maxHealthPoint;
            playerManager.currentHealth = player.health;
            Vector2 position;
            position.x = player.position[0];
            position.y = player.position[1];
            playerManager.transform.parent.position = position;

            playerManager.UpdateHealthBar();
        }
    }
}
