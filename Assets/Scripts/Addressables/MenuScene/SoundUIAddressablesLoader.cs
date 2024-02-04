using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SoundUI))]
public class SoundUIAddressablesLoader : MonoBehaviour, IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 2;

    [SerializeField] private Image _musicImage; 
    [SerializeField] private Image _musicCrossImage;
    [SerializeField] private Image _sfxCrossImage;

    private readonly List<Sprite> _loadedSprites = new();

    private void OnApplicationQuit()
    {
        UnloadContent();
    }

    public async Task LoadContent()
    {
        Sprite musicSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Music);
        _musicImage.sprite = musicSprite;
        LoadedCount++;
        
        Sprite crossSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Cross);
        _musicCrossImage.sprite = crossSprite;
        _sfxCrossImage.sprite = crossSprite;
        LoadedCount++;
        
        _loadedSprites.Add(musicSprite);
        _loadedSprites.Add(crossSprite);
    }

    public void UnloadContent()
    {
        foreach (Sprite sprite in _loadedSprites)
        {
            AddressablesLoader.Unload(sprite);
        }

        LoadedCount = 0;
    }
}
