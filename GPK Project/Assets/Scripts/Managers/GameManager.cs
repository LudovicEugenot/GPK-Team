using System.Collections;
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
    public BeatManager Beat; //majuscule parce que manager
    public GameObject player;
    [HideInInspector] public Blink blink;
    #endregion

    private void Start()
    {
        ProgressionManager.currentProgression = ProgressionManager.ProgressionState.Tutorial1;
        blink = player.GetComponentInChildren<Blink>();
    }
}
