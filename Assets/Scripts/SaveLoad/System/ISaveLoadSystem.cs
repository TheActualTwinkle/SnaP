public interface ISaveLoadSystem
{
    void Save(ISaveLoadData saveLoadData);
    T Load<T>() where T : ISaveLoadData;
}
