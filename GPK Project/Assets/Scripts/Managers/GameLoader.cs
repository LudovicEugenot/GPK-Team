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
    public string saveFileExtension;
    public string defaultSaveDirectoryName;

    private WorldData worldData;
    private PlayerData playerData;

    public void StartNewGame()
    {
        SetupSaveSystem();
        SceneManager.LoadScene(startSceneBuildIndex);
    }

    public void LoadGame()
    {
        SetupSaveSystem();
        LoadWorldData();
        LoadPlayerData();
        SceneManager.LoadScene(worldData.savedZoneBuildIndex);
    }

    private void SetupSaveSystem()
    {
        SaveSystem.SetSavePath(specifiedSaveFilePath);
        SaveSystem.playerDataSaveFileName = playerDataSaveFileName;
        SaveSystem.worldDataSaveFileName = worldDataSaveFileName;
        SaveSystem.saveFileExtension = saveFileExtension;
        SaveSystem.defaultSaveDirectoryName = defaultSaveDirectoryName;
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
