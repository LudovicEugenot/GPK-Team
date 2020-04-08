using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ZoneHandler : MonoBehaviour
{
    [HideInInspector] public bool isCurrentConverted;
    [HideInInspector] public Zone currentZone;
    [HideInInspector] public List<Zone> zones = new List<Zone>();

    #region Singleton
    public static ZoneHandler Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    #endregion

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.O))
        {
            isCurrentConverted = true;
            currentZone.isConverted = true;
        }

        UpdateConversion();
        UpdateZoneState();
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
                currentZone.isConverted = true;
                Debug.Log("The zone " + currentZone.name + " is relived ! ", gameObject);
            }
        }
    }

    private void UpdateZoneState()
    {
        for(int i = 0; i < currentZone.enemiesDead.Length; i++)
        {
            //if(GameManager.Instance.zoneEnemies[i].)
        }
    }

    public void InitializeZone(Zone newZone)
    {
        currentZone = newZone;
        isCurrentConverted = newZone.isConverted;

        for(int i = 0; i < currentZone.enemiesDead.Length; i++)
        {
            if(currentZone.enemiesDead[i])
            {
                GameManager.Instance.zoneEnemies[i].gameObject.SetActive(false);
            }
        }


    }

    [System.Serializable]
    public class Zone
    {
        public bool isConverted;
        public int buildIndex;
        public string name;
        public List<Hook> zoneHooks;
        public bool[] enemiesDead;
        public bool[] elementsActivated;

        public Zone(int _buildIndex, string zoneName, List<Hook> _zoneHooks, int enemyNumber, int elementNumber)
        {
            buildIndex = _buildIndex;
            isConverted = false;
            name = zoneName;
            zoneHooks = _zoneHooks;
            enemiesDead = new bool[enemyNumber];
            elementsActivated = new bool[elementNumber];
        }
    }
}
