using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ZoneHandler : MonoBehaviour
{
    [HideInInspector] public bool isCurrentConverted;
    [HideInInspector] public Zone currentZone;
    [HideInInspector] public List<Zone> zones = new List<Zone>();

    private TransitionManager transitionManager;

    #region Singleton
    public static ZoneHandler Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        transitionManager = GetComponent<TransitionManager>();
    }
    #endregion

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            isCurrentConverted = true;
            currentZone.isConverted = true;
        }
    }

    public void InitializeZone(Zone newZone)
    {
        currentZone = newZone;
        isCurrentConverted = newZone.isConverted;

        Debug.Log("Zone" + currentZone.name + " is " + (currentZone.isConverted ? "converted" : " still in sadness :,("));
    }

    [System.Serializable]
    public class Zone
    {
        public bool isConverted;
        public int buildIndex;
        public string name;

        public Zone(int _buildIndex, string zoneName)
        {
            buildIndex = _buildIndex;
            isConverted = false;
            name = zoneName;
        }
    }
}
