using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static string playerDataSaveFileName;
    public static string worldDataSaveFileName;
    public static string previewDataSaveFileName;
    public static string screenPreviewFileName;
    public static string saveFileExtension;
    public static string defaultSaveDirectoryName;

    private static string savePath;


    /// <summary>
    /// Create the path to the directory where the save files will be created
    /// </summary>
    /// <param name="alternateSavePath">Create a default save path if set to null or empty</param>
    public static void SetSavePath(string alternateSavePath)
    {
        if (alternateSavePath != null && alternateSavePath != "")
        {
            savePath = alternateSavePath;
        }
        else
        {
            savePath = Application.persistentDataPath + defaultSaveDirectoryName;
            Directory.CreateDirectory(savePath);
        }
    }

    public static void SavePlayer(PlayerManager player)
    {
        if(savePath != "" && savePath != null)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = savePath + playerDataSaveFileName + saveFileExtension;

            FileStream stream = new FileStream(path, FileMode.Create);

            PlayerData playerData = new PlayerData(player);

            formatter.Serialize(stream, playerData);
            stream.Close();

            Debug.Log("Player saved in " + path);
        }
        else
        {
            Debug.LogError("The savePath has not been set");
        }
    }

    public static PlayerData LoadPlayer()
    {
        string path = savePath + playerDataSaveFileName + saveFileExtension;

        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData playerData = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            Debug.Log("PLayer loaded from " + path);

            return playerData;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    public static void SaveWorld(ZoneHandler zoneHandler)
    {
        if (savePath != "" && savePath != null)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = savePath + worldDataSaveFileName + saveFileExtension;

            FileStream stream = new FileStream(path, FileMode.Create);

            WorldData worldData = new WorldData(zoneHandler);

            formatter.Serialize(stream, worldData);
            stream.Close();

            Debug.Log("World saved in " + path);
        }
        else
        {
            Debug.LogError("The savePath has not been set");
        }
    }

    public static WorldData LoadWorld()
    {
        string path = savePath + worldDataSaveFileName + saveFileExtension;

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream stream = new FileStream(path, FileMode.Open);

            WorldData worldData = formatter.Deserialize(stream) as WorldData;
            stream.Close();

            Debug.Log("World loaded from " + path);

            return worldData;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

    public static void SavePreview(ZoneHandler zoneHandler, Texture2D screenTexture)
    {
        if (savePath != "" && savePath != null)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = savePath + previewDataSaveFileName + saveFileExtension;

            FileStream stream = new FileStream(path, FileMode.Create);

            PreviewData previewData = new PreviewData(zoneHandler, screenTexture);

            formatter.Serialize(stream, previewData);
            stream.Close();

            Debug.Log("Preview saved in " + path);
        }
        else
        {
            Debug.LogError("The savePath has not been set");
        }
    }

    public static PreviewData LoadPreview()
    {
        string path = savePath + previewDataSaveFileName + saveFileExtension;

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream stream = new FileStream(path, FileMode.Open);

            PreviewData previewData = formatter.Deserialize(stream) as PreviewData;
            stream.Close();

            Debug.Log("Preview loaded from " + path);

            return previewData;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
