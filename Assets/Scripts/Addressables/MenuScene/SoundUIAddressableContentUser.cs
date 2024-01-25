using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SoundUI))]
public class SoundUIAddressableContentUser : MonoBehaviour, IAddressableContentUser
{
    [SerializeField] private Image _musicImage; 
    [SerializeField] private Image _musicCrossImage;
    [SerializeField] private Image _sfxCrossImage;

    private readonly List<Sprite> _loadedSprites = new();

    private async void Start()
    {
        await LoadContent();
    }

    private void OnDestroy()
    {
        UnloadContent();
    }

    public async Task LoadContent()
    {
        Sprite musicSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Music);
        _musicImage.sprite = musicSprite;
        
        Sprite crossSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Cross);
        _musicCrossImage.sprite = crossSprite;
        _sfxCrossImage.sprite = crossSprite;
        
        _loadedSprites.Add(musicSprite);
        _loadedSprites.Add(crossSprite);
    }

    public void UnloadContent()
    {
        foreach (Sprite sprite in _loadedSprites)
        {
            AddressablesLoader.Unload(sprite);
        }
    }
}
