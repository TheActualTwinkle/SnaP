using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GameMainIconAddressableContentUser : MonoBehaviour, IAddressableContentUser
{
    [SerializeField] private Image _backgroundImage;

    private Sprite _loadedSprite;
    
    private void Start()
    {
        LoadContent();
    }

    private void OnDestroy()
    {
        UnloadContent();
    }

    public async void LoadContent()
    {
        _loadedSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.GameMainIcon);
        _backgroundImage.sprite = _loadedSprite;
    }

    public void UnloadContent()
    {
        AddressablesLoader.Unload(_loadedSprite);
    }
}
