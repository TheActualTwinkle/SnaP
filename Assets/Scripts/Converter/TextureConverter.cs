using UnityEngine;

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

    public static byte[] GetRawTexture(Texture2D texture)
    {
        return texture.EncodeToPNG();
    }
    
    private static Texture2D GetTexture2D(byte[] data)
    {
        Texture2D texture = new(2, 2);
        texture.LoadImage(data);

        return texture;
    }
}
