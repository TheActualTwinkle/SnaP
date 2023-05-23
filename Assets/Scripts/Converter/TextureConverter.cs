using UnityEngine;

public static class TextureConverter
{
    public static Sprite GetSprite(byte[] data, int width, int height)
    {
        return GetSprite(GetTexture2D(data), width, height);
    }

    public static Sprite GetSprite(Texture2D texture, int width, int height)
    {
        Texture2D newTexture = ResizeTexture(texture, width, height);
        return Sprite.Create(newTexture, new Rect(0f, 0f, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f), 64f);
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
    
    private static Texture2D ResizeTexture(Texture2D originalTexture, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        RenderTexture.active = rt;

        Graphics.Blit(originalTexture, rt);

        Texture2D resizedTexture = new(width, height);
        resizedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        resizedTexture.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return resizedTexture;
    }
}
