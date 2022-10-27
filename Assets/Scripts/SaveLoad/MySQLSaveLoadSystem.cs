using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MySQLSaveLoadSystem : ISaveLoadSystem
{
    public void Save(ISaveLoadData data)
    {
        throw new System.NotImplementedException();
    }

    public T Load<T>() where T : ISaveLoadData
    {
        throw new System.NotImplementedException();
    }
}