using Unity.Netcode;
// ReSharper disable InconsistentNaming

[System.Serializable]
public struct PlayerData : ISaveLoadData, INetworkSerializable
{
    public string NickName;
    public string AvatarBase64String;
    public uint Stack;
    
    public PlayerData(string nickName, string avatarBase64String, uint stack)
    {
        NickName = nickName;
        AvatarBase64String = avatarBase64String;
        Stack = stack;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref NickName);
        serializer.SerializeValue(ref AvatarBase64String);
        serializer.SerializeValue(ref Stack);
    }
}