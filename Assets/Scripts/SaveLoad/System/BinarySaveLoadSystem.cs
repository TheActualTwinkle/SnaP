using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class BinarySaveLoadSystem : ISaveLoadSystem
{
    private static readonly string SavePath = Application.persistentDataPath + "/saves";
    private const string SaveFileExtension = "df";

    private static readonly BinaryFormatter BinaryFormatter = new();

    public void Save(ISaveLoadData saveLoadData)
    {
        if (Directory.Exists(SavePath) == false)
        {
            Directory.CreateDirectory(SavePath);
        }

        var saveFileName = $"{SavePath}/{saveLoadData.GetType().Name}.{SaveFileExtension}";
        FileStream fileStream = new(saveFileName, FileMode.Create);

        BinaryFormatter.Serialize(fileStream, saveLoadData);

        fileStream.Close();
    }

    public T Load<T>() where T : ISaveLoadData, new()
    {
        var saveFileName = $"{SavePath}/{typeof(T).Name}.{SaveFileExtension}";
        if (File.Exists(saveFileName) == false)
        {
            T t = new();
            t.SetDefaultValues();
            return t;
        }
        
        FileStream fileStream = new(saveFileName, FileMode.Open);

        try
        {
            ISaveLoadData data = BinaryFormatter.Deserialize(fileStream) as ISaveLoadData;

            return (T)data;
        }
        catch
        {
            T t = new();
            t.SetDefaultValues();
            return t;
        }
        finally
        {
            fileStream.Close();
        }
    }
}
