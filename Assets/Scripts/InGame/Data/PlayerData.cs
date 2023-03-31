using System;
using Unity.Netcode;
using UnityEngine;

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

    public void SetDefault()
    {
        NickName = "Player";
        
        byte[] texture = TextureConverter.GetRawTexture(Resources.Load<Sprite>("Sprites/ava").texture);
        AvatarBase64String = Convert.ToBase64String(texture);

        Stack = 0;
    }
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref NickName);
        serializer.SerializeValue(ref AvatarBase64String);
        serializer.SerializeValue(ref Stack);
    }
}