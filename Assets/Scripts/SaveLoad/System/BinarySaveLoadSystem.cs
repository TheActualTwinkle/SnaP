using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class BinarySaveLoadSystem : ISaveLoadSystem
{
    private static readonly string _savePath = Application.persistentDataPath + "/saves";
    private static readonly string _saveFileExtension = "df";

    private static BinaryFormatter _binatyFormatter = new BinaryFormatter();

    public void Save(ISaveLoadData saveLoadData)
    {
        if (Directory.Exists(_savePath) == false)
        {
            Directory.CreateDirectory(_savePath);
        }

        string saveFileName = $"{_savePath}/{saveLoadData.GetType().Name}.{_saveFileExtension}";
        FileStream fileStream = new FileStream(saveFileName, FileMode.Create);

        _binatyFormatter.Serialize(fileStream, saveLoadData);

        fileStream.Close();
    }

    public T Load<T>() where T : ISaveLoadData
    {
        string saveFileName = $"{_savePath}/{typeof(T).Name}.{_saveFileExtension}";
        if (File.Exists(saveFileName) == true)
        {
            FileStream fileStream = new FileStream(saveFileName, FileMode.Open);

            ISaveLoadData data = _binatyFormatter.Deserialize(fileStream) as ISaveLoadData;

            fileStream.Close();

            return (T)data;
        }
        else
        {
            return default;
        }
    }
}
