[System.Serializable]
public struct PlayerData : ISaveLoadData
{
    public readonly string NickName;
    public readonly string AvatarBase64String;

    public PlayerData(string nickName, string avatarBase64String)
    {
        NickName = nickName;
        AvatarBase64String = avatarBase64String;
    }
}