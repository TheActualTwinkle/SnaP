using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public readonly string Name;
    public readonly string ImageID;

    public PlayerData(string name, string imageID)
    {
        Name = name;
        ImageID = imageID;
    }
}
