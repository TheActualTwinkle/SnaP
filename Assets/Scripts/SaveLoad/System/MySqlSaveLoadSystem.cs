using System;

public class MySqlSaveLoadSystem : ISaveLoadSystem
{   
    public void Save(ISaveLoadData data)
    {
        throw new NotImplementedException();
    }

    public T Load<T>() where T : ISaveLoadData
    {
        throw new NotImplementedException();
    }
}