using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class BinarySaveLoadSystem : ISaveLoadSystem
{
    private readonly static string _savePath = Application.persistentDataPath + "/saves";
    private readonly static string _saveFileExtension = ".df";

    private static BinaryFormatter _binatyFormatter = new BinaryFormatter();

    public void Save(ISaveLoadData saveLoadData)
    {
        if (Directory.Exists(_savePath) == false)
        {
            Directory.CreateDirectory(_savePath);
        }

        string saveFileName = $"{_savePath}/{saveLoadData.GetType().Name}{_saveFileExtension}";
        FileStream fileStream = new FileStream(saveFileName, FileMode.Create);

        _binatyFormatter.Serialize(fileStream, saveLoadData);

        fileStream.Close();
    }

    public T Load<T>() where T : ISaveLoadData
    {
        string saveFileName = $"{_savePath}/{typeof(T).Name}{_saveFileExtension}";
        if (File.Exists(saveFileName) == true)
        {
            return (T)Load(saveFileName);
        }
        else
        {
            return default;
        }
    }

    private ISaveLoadData Load(string saveFileName)
    {
        FileStream fileStream = new FileStream(saveFileName, FileMode.Open);

        ISaveLoadData data = _binatyFormatter.Deserialize(fileStream) as ISaveLoadData;

        fileStream.Close();

        return data;
    }
}
