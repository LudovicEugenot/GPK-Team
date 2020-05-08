using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ZoneHandler : MonoBehaviour
{
    [HideInInspector] public bool isInstrumentPresent;
    [HideInInspector] public Zone currentZone;
    [HideInInspector] public float currentReliveProgression;
    [HideInInspector] public List<Zone> zones = new List<Zone>();

    [HideInInspector] public bool zoneInitialized;
    private bool isAnimatingRecolor;

    #region Singleton
    public static ZoneHandler Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (Instance == null)
        {
            Instance = this;
            GeneralInitialization();

            zoneInitialized = false;
        }
        else
        {
            Destroy(this);
        }
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
            currentZone.isRelived = true;
        }

        if(zoneInitialized)
        {
            UpdateRelive();
        }
    }

    private void UpdateRelive()
    {
        int hooksRelived = 0;
        if(!currentZone.isRelived)
        {
            bool zoneRelived = true;
            foreach(HookState zoneHook in currentZone.zoneHooks)
            {
                if(!zoneHook.relived)
                {
                    zoneRelived = false;
                }
                else
                {
                    hooksRelived++;
                }
            }

            if(zoneRelived)
            {
                currentZone.isRelived = true;
                StartCoroutine(RecolorEffect());
            }
        }
        else
        {
            hooksRelived = currentZone.zoneHooks.Count;
        }

        if(!isInstrumentPresent && !isAnimatingRecolor)
        {
            currentReliveProgression = (float)hooksRelived / (float)currentZone.zoneHooks.Count;
        }
    }

    public IEnumerator RecolorEffect()
    {
        //Jouer son recolor
        isAnimatingRecolor = true;
        StartCoroutine(GameManager.Instance.cameraHandler.CinematicLook(Vector2.zero, 2.0f, 5.625f, true));
        currentReliveProgression = 0;
        yield return new WaitForSeconds(1.0f);
        currentReliveProgression = 1;
        // animation de recolor
        for (int i = 0; i < GameManager.Instance.recolorHealthHealed; i++)
        {
            GameManager.Instance.playerManager.Heal(1);
            yield return new WaitForSeconds(0.2f);
        }
        isAnimatingRecolor = false;
    }

    public void SaveZoneState()
    {
        for (int i = 0; i < currentZone.enemiesConverted.Length; i++)
        {
            currentZone.enemiesConverted[i] = GameManager.Instance.zoneEnemies[i].IsConverted();
        }

        for (int i = 0; i < currentZone.elementsEnabled.Length; i++)
        {
            if (i < GameManager.Instance.zoneElements.Count)
            {
                currentZone.elementsEnabled[i] = GameManager.Instance.zoneElements[i].isEnabled;
            }
        }

        for (int i = 0; i < currentZone.hooksRelived.Length; i++)
        {
            currentZone.hooksRelived[i] = currentZone.zoneHooks[i].relived;
        }


        for (int i = 0; i < currentZone.heartContainersObtained.Length; i++)
        {
            currentZone.heartContainersObtained[i] = GameManager.Instance.heartContainers[i].isObtained;
        }
    }

    public void InitializeZone(Zone newZone)
    {
        currentZone = newZone;
        isInstrumentPresent = false;

        for(int i = 0; i < currentZone.enemiesConverted.Length; i++)
        {
            GameManager.Instance.zoneEnemies[i].transform.parent.gameObject.SetActive(!currentZone.enemiesConverted[i]);
            if(currentZone.enemiesConverted[i])
            {
                GameManager.Instance.zoneEnemies[i].GetConverted(true);
            }
        }


        for (int i = 0; i < currentZone.elementsEnabled.Length; i++)
        {
            if(i < GameManager.Instance.zoneElements.Count)
            {
                GameManager.Instance.zoneElements[i].isEnabled = currentZone.elementsEnabled[i];
            }
        }

        for (int i = 0; i < currentZone.heartContainersObtained.Length; i++)
        {
            if (i < GameManager.Instance.heartContainers.Count)
            {
                GameManager.Instance.heartContainers[i].isObtained = currentZone.heartContainersObtained[i];
            }
        }

        currentZone.zoneHooks = GameManager.Instance.zoneHooks;
        for (int i = 0; i < currentZone.hooksRelived.Length; i++)
        {
            if(currentZone.hooksRelived[i])
            {
                currentZone.zoneHooks[i].Relive();
            }
        }

        GameManager.Instance.playerManager.currentPower = 0;
        GameManager.Instance.zoneHandler = this;

        zoneInitialized = true;
    }

    public bool AllEnemiesConverted()
    {
        SaveZoneState();
        foreach (bool enemyConverted in currentZone.enemiesConverted)
        {
            if (!enemyConverted)
            {
                return false;
            }
        }
        return true;
    }

    [System.Serializable]
    public class Zone
    {
        public bool isRelived;
        public int buildIndex;
        public string name;
        public List<HookState> zoneHooks;
        public bool[] hooksRelived;
        public bool[] enemiesConverted;
        public bool[] elementsEnabled;
        public bool[] heartContainersObtained;

        public Zone(int _buildIndex, string zoneName, List<HookState> _zoneHooks, int enemyNumber, int elementNumber, int heartContainerNumber)
        {
            buildIndex = _buildIndex;
            isRelived = false;
            name = zoneName;
            zoneHooks = _zoneHooks;
            hooksRelived = new bool[zoneHooks.Count];
            enemiesConverted = new bool[enemyNumber];
            elementsEnabled = new bool[elementNumber];
            heartContainersObtained = new bool[heartContainerNumber];
        }
    }
}
