using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerData
{
    public readonly string NickName;
    public readonly string ImageID;

    public PlayerData(string nickName, string imageID)
    {
        NickName = nickName;
        ImageID = imageID;
    }
}