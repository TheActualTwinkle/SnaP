using Unity.Netcode;

// ReSharper disable InconsistentNaming

[System.Serializable]
public struct PlayerData : ISaveLoadData, INetworkSerializable
{
    public string NickName => _nickName;
    private string _nickName;

    public uint Stack => _stack;
    private uint _stack;

    public PlayerData(string nickName, uint stack)
    {
        _nickName = nickName;
        _stack = stack;
    }

    public void SetDefaultValues()
    {
        _nickName = "Player";
        _stack = 100;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _nickName);
        serializer.SerializeValue(ref _stack);
    }
}