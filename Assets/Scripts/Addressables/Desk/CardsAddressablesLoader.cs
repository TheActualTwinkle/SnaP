using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardsAddressablesLoader : MonoBehaviour, IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 13 * 4 + 1; // 13 cards * 4 suits + 1 card back.

    private readonly List<Sprite> _loadedSprites = new();

    private void OnDestroy()
    {
        UnloadContent();
    }
    
    public async Task LoadContent()
    {
        for (var i = 0; i < Enum.GetValues(typeof(Suit)).Length; i++)
        {
            for (var j = 2; j < Enum.GetValues(typeof(Value)).Length + 2; j++)
            {
                Sprite sprite = await AddressablesLoader.LoadAsync<Sprite>($"{j}_{(Suit)i}");
                _loadedSprites.Add(sprite);
                LoadedCount++;
            }
        }
        
        Sprite cardBackSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Cards.CardBack);
        _loadedSprites.Add(cardBackSprite);
        LoadedCount++;
    }

    public void UnloadContent()
    {
        foreach (Sprite sprite in _loadedSprites)
        {
            AddressablesLoader.Unload(sprite);
            LoadedCount--;
        }
    }
}
