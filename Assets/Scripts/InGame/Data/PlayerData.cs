using Unity.Netcode;

[System.Serializable]
public struct PlayerData : ISaveLoadData, INetworkSerializable
{
    public string _nickName;
    public string _avatarBase64String;
    public uint _stack;
    
    public PlayerData(string nickName, string avatarBase64String, uint stack)
    {
        _nickName = nickName;
        _avatarBase64String = avatarBase64String;
        _stack = stack;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _nickName);
        serializer.SerializeValue(ref _avatarBase64String);
        serializer.SerializeValue(ref _stack);
    }
}