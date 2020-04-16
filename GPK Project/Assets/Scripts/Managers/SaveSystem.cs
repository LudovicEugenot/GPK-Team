using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static string alternatePath;

    public static void SavePlayer(PlayerManager player, string alternateSavePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path;
        if (alternateSavePath != null && alternateSavePath != "")
        {
            path = alternateSavePath;
        }
        else
        {
            path = Application.persistentDataPath + "/Saves";
            Directory.CreateDirectory(path);
        }
        path += "/playerData.save";

        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData playerData = new PlayerData(player);

        formatter.Serialize(stream, playerData);
        stream.Close();

        Debug.Log("PLayer saved in " + path);
    }

    public static PlayerData LoadPlayer(string alternateSavePath)
    {
        string path;
        if (alternateSavePath != null && alternateSavePath != "")
        {
            path = alternateSavePath;
        }
        else
        {
            path = Application.persistentDataPath + "/Saves";
        }
        path += "/playerData.save";

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
}
