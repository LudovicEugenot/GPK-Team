using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ZoneHandler : MonoBehaviour
{
    [HideInInspector] public bool isCurrentConverted;
    [HideInInspector] public Zone currentZone;
    [HideInInspector] public List<Zone> zones = new List<Zone>();

    private bool zoneInitialized;

    #region Singleton
    public static ZoneHandler Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        zoneInitialized = false;
        GeneralInitialization();
    }
    #endregion

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void GeneralInitialization()
    {
        WorldManager.InitializeWorldEvents();
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.O))
        {
            isCurrentConverted = true;
            currentZone.isRelived = true;
        }

        if(zoneInitialized)
        {
            UpdateConversion();
            UpdateZoneState();
        }
    }

    private void UpdateConversion()
    {
        if(!isCurrentConverted)
        {
            bool zoneRelived = true;
            foreach(Hook zoneHook in currentZone.zoneHooks)
            {
                if(!zoneHook.relived)
                {
                    zoneRelived = false;
                }
            }

            if(zoneRelived)
            {
                isCurrentConverted = true;
                currentZone.isRelived = true;
            }
        }
    }

    private void UpdateZoneState()
    {
        for(int i = 0; i < currentZone.enemiesConverted.Length; i++)
        {
            currentZone.enemiesConverted[i] = GameManager.Instance.zoneEnemies[i].IsConverted();
        }

        for (int i = 0; i < currentZone.elementsActivated.Length; i++)
        {
            currentZone.elementsActivated[i] = GameManager.Instance.zoneElements[i].active ? GameManager.Instance.zoneElements[i].enableState : !GameManager.Instance.zoneElements[i].enableState;
        }
    }

    public void InitializeZone(Zone newZone)
    {
        currentZone = newZone;
        isCurrentConverted = newZone.isRelived;

        for(int i = 0; i < currentZone.enemiesConverted.Length; i++)
        {
            GameManager.Instance.zoneEnemies[i].transform.parent.gameObject.SetActive(!currentZone.enemiesConverted[i]);
        }

        for (int i = 0; i < currentZone.elementsActivated.Length; i++)
        {
            GameManager.Instance.zoneElements[i].active = currentZone.elementsActivated[i] ? GameManager.Instance.zoneElements[i].enableState : !GameManager.Instance.zoneElements[i].enableState;
        }

        currentZone.zoneHooks = GameManager.Instance.zoneHooks;

        zoneInitialized = true;
    }

    [System.Serializable]
    public class Zone
    {
        public bool isRelived;
        public int buildIndex;
        public string name;
        public List<Hook> zoneHooks;
        public bool[] enemiesConverted;
        public bool[] elementsActivated;

        public Zone(int _buildIndex, string zoneName, List<Hook> _zoneHooks, int enemyNumber, int elementNumber)
        {
            buildIndex = _buildIndex;
            isRelived = false;
            name = zoneName;
            zoneHooks = _zoneHooks;
            enemiesConverted = new bool[enemyNumber];
            elementsActivated = new bool[elementNumber];
        }
    }
}
