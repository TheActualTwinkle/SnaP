using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class BinarySaveLoadSystem : ISaveLoadSystem
{
    private static readonly string SavePath = Application.persistentDataPath + "/saves";
    private const string SaveFileExtension = "df";

    private static BinaryFormatter _binatyFormatter = new();

    public void Save(ISaveLoadData saveLoadData)
    {
        if (Directory.Exists(SavePath) == false)
        {
            Directory.CreateDirectory(SavePath);
        }

        var saveFileName = $"{SavePath}/{saveLoadData.GetType().Name}.{SaveFileExtension}";
        FileStream fileStream = new(saveFileName, FileMode.Create);

        _binatyFormatter.Serialize(fileStream, saveLoadData);

        fileStream.Close();
    }

    public T Load<T>() where T : ISaveLoadData
    {
        var saveFileName = $"{SavePath}/{typeof(T).Name}.{SaveFileExtension}";
        if (File.Exists(saveFileName) == true)
        {
            FileStream fileStream = new(saveFileName, FileMode.Open);

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
