using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    public int startSceneBuildIndex;
    public GameObject musicManager;
    [Space]
    [Tooltip("Leave empty to use default save file")] public string specifiedSaveFilePath; 
    public string playerDataSaveFileName;
    public string worldDataSaveFileName;
    public string previewDataSaveFileName;
    public string saveFileExtension;
    public string defaultSaveDirectoryName;
    public string defaultGameDirectoryName;
    [HideInInspector] public string customSaveDirectory;


    [HideInInspector] public WorldData worldData;
    [HideInInspector] public PlayerData playerData;
    private MainMenuManager mainMenuManager;

    private void Start()
    {
        customSaveDirectory = "";
        mainMenuManager = GetComponent<MainMenuManager>();
        SetupSaveSystem();
    }

    public void StartNewGame()
    {
        //StartMusicManager();
        SceneManager.LoadScene(startSceneBuildIndex);
    }

    public void LoadSelectedGame()
    {
        customSaveDirectory = mainMenuManager.directoryNameSelected;
        LoadGame();
    }

    public void LoadGame()
    {
        LoadWorldData();
        LoadPlayerData();
        //StartMusicManager();
        StartGame();
    }

    public void StartGame()
    {
        if(worldData != null && playerData != null)
        {
            SceneManager.LoadScene(worldData.savedZoneBuildIndex);
        }
        else
        {
            Debug.LogError("Save data has not been loaded");
        }
    }

    private void SetupSaveSystem()
    {
        SaveSystem.defaultSaveDirectoryName = defaultSaveDirectoryName;
        SaveSystem.defaultGameDirectoryName = defaultGameDirectoryName;
        SaveSystem.SetSavePath(specifiedSaveFilePath);
        SaveSystem.playerDataSaveFileName = playerDataSaveFileName;
        SaveSystem.worldDataSaveFileName = worldDataSaveFileName;
        SaveSystem.saveFileExtension = saveFileExtension;
        SaveSystem.previewDataSaveFileName = previewDataSaveFileName;
    }

    private void LoadWorldData()
    {
        worldData = customSaveDirectory == "" ? SaveSystem.LoadWorld() : SaveSystem.LoadWorld(customSaveDirectory);
        ZoneHandler.Instance.zones = new List<ZoneHandler.Zone>(worldData.worldZones);
        WorldManager.allWorldEvents = new List<WorldManager.WorldEvent>(worldData.worldEvents);
        WorldManager.currentStoryStep = worldData.storyStep;
    }

    private void LoadPlayerData()
    {
        playerData = customSaveDirectory == "" ? SaveSystem.LoadPlayer() : SaveSystem.LoadPlayer(customSaveDirectory);
        TransitionManager.Instance.previousPlayerData = playerData;
    }

    private void StartMusicManager()
    {
        musicManager.SetActive(true);
    }
}
