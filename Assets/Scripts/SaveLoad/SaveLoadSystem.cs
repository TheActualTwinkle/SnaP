using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class SaveLoadSystem
{
    public static string PlayerSaveFile => _playerSaveFile;
    private readonly static string _playerSaveFile = Application.persistentDataPath + "/saves/playerData.df";

    private static BinaryFormatter _binatyFormatter = new BinaryFormatter();

    public static void SavePlayerData(PlayerData data)
    {
        FileStream fileStream = new FileStream(_playerSaveFile, FileMode.Create);

        _binatyFormatter.Serialize(fileStream, data);

        fileStream.Close();
    }

    public static PlayerData LoadPlayerData()
    {
        if (File.Exists(_playerSaveFile))
        {
            FileStream fileStream = new FileStream(_playerSaveFile, FileMode.Open);

            PlayerData playerData = _binatyFormatter.Deserialize(fileStream) as PlayerData;

            fileStream.Close();

            return playerData;
        }
        else
        {
            return null;
        }
    }
}
