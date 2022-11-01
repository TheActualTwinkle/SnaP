using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public static class TextureConverter
{
    public static Sprite GetSprite(byte[] data)
    {
        return GetSprite(GetTexture2D(data));
    }

    public static Sprite GetSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 64f);
    }

    public static Texture2D GetTexture2D(byte[] data)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(data);

        return texture;
    }

    public static byte[] GetRawTexture(Texture2D texture)
    {
        return texture.EncodeToPNG();
    }
}
