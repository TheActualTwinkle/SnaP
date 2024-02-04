using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChipsAddressablesLoader : MonoBehaviour, IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => Constants.Sprites.Chips.AssetsCount; // 7 chips stack. (Pot is included as ChipsStack_7)

    private readonly List<Sprite> _loadedSprites = new();

    private void OnDestroy()
    {
        UnloadContent();
    }
    
    public async Task LoadContent()
    {
        for (var i = 0; i < Constants.Sprites.Chips.AssetsCount; i++)
        {
            Sprite sprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Chips.ChipsStack + (i + 1));
            _loadedSprites.Add(sprite);
            LoadedCount++;
        }
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
