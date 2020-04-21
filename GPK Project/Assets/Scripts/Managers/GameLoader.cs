using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    public int startSceneBuildIndex;
    [Space]
    [Tooltip("Leave empty to use default save file")] public string specifiedSaveFilePath; 
    public string playerDataSaveFileName;
    public string worldDataSaveFileName;
    public string previewDataSaveFileName;
    public string screenPreviewFileName;
    public string saveFileExtension;
    public string defaultSaveDirectoryName;


    [HideInInspector] public WorldData worldData;
    [HideInInspector] public PlayerData playerData;

    private void Start()
    {
        SetupSaveSystem();
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene(startSceneBuildIndex);
    }

    public void LoadGame()
    {
        LoadWorldData();
        LoadPlayerData();

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
        SaveSystem.SetSavePath(specifiedSaveFilePath);
        SaveSystem.playerDataSaveFileName = playerDataSaveFileName;
        SaveSystem.worldDataSaveFileName = worldDataSaveFileName;
        SaveSystem.saveFileExtension = saveFileExtension;
        SaveSystem.defaultSaveDirectoryName = defaultSaveDirectoryName;
        SaveSystem.previewDataSaveFileName = previewDataSaveFileName;
        SaveSystem.screenPreviewFileName = screenPreviewFileName;
    }

    private void LoadWorldData()
    {
        worldData = SaveSystem.LoadWorld();
        ZoneHandler.Instance.zones = worldData.worldZones;
        WorldManager.allWorldEvents = worldData.worldEvents;
    }

    private void LoadPlayerData()
    {
        playerData = SaveSystem.LoadPlayer();
        TransitionManager.Instance.savePos.x = playerData.position[0];
        TransitionManager.Instance.savePos.y = playerData.position[1];
        TransitionManager.Instance.newPlayerHp = playerData.health;
    }
}
