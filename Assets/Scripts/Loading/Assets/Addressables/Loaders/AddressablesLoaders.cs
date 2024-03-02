using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

// ReSharper disable ClassNeverInstantiated.Global

// Classes for loading assets via Addressables.
// Based on Metadata and Factory patterns.

#region MenuScene

[Export(typeof(IAddressablesLoader))]
[ExportMetadata(nameof(AddressablesLoaderMetadata.OperableSceneNames), new[] {Constants.SceneNames.Menu})]
[ExportMetadata(nameof(AddressablesLoaderMetadata.IsExclusive), false)]
public class GameMainIconAddressablesLoader : IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 1;

    public Sprite Sprite { get; private set; }

    public async Task LoadContent()
    {
        Sprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.GameMainIcon);
        LoadedCount = 1;
    }

    public void UnloadContent()
    {
        AddressablesAssetLoader.Unload(Sprite);
        LoadedCount = 0;
    }
}

#endregion

#region DeskScene

[Export(typeof(IAddressablesLoader))]
[ExportMetadata(nameof(AddressablesLoaderMetadata.OperableSceneNames), new[] {Constants.SceneNames.Desk})]
[ExportMetadata(nameof(AddressablesLoaderMetadata.IsExclusive), false)]
public class CardsAddressablesLoader : IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 13 * 4 + 1; // 13 cards * 4 suits + 1 card back.

    public List<Sprite> Sprites { get; } = new();
    
    public async Task LoadContent()
    {
        for (var i = 0; i < Enum.GetValues(typeof(Suit)).Length; i++)
        {
            for (var j = 2; j < Enum.GetValues(typeof(Value)).Length + 2; j++)
            {
                Sprite sprite = await AddressablesAssetLoader.LoadAsync<Sprite>($"{j}_{(Suit)i}");
                Sprites.Add(sprite);
                LoadedCount++;
            }
        }
        
        Sprite cardBackSprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.Cards.CardBack);
        Sprites.Add(cardBackSprite);
        LoadedCount++;
    }

    public void UnloadContent()
    {
        foreach (Sprite sprite in Sprites)
        {
            AddressablesAssetLoader.Unload(sprite);
            LoadedCount--;
        }
    }
}

[Export(typeof(IAddressablesLoader))]
[ExportMetadata(nameof(AddressablesLoaderMetadata.OperableSceneNames), new[] {Constants.SceneNames.Desk})]
[ExportMetadata(nameof(AddressablesLoaderMetadata.IsExclusive), false)]
public class ChipsAddressablesLoader : IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => Constants.Sprites.Chips.AssetsCount; // 7 chips stack. (Pot is included as ChipsStack_7)

    public List<Sprite> Sprites { get; } = new();

    public async Task LoadContent()
    {
        for (var i = 0; i < Constants.Sprites.Chips.AssetsCount; i++)
        {
            Sprite sprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.Chips.ChipsStackTemplate + (i + 1));
            Sprites.Add(sprite);
            LoadedCount++;
        }
    }

    public void UnloadContent()
    {
        foreach (Sprite sprite in Sprites)
        {
            AddressablesAssetLoader.Unload(sprite);
            LoadedCount--;
        }
    }
}

[Export(typeof(IAddressablesLoader))]
[ExportMetadata(nameof(AddressablesLoaderMetadata.OperableSceneNames), new[] {Constants.SceneNames.Desk})]
[ExportMetadata(nameof(AddressablesLoaderMetadata.IsExclusive), false)]
public class SfxAudioAddressablesLoader : IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => (uint)Constants.Sound.Sfx.Paths.Count;

    public Dictionary<Constants.Sound.Sfx.Type, AudioClip> Clips { get; } = new();

    public async Task LoadContent()
    {
        foreach (KeyValuePair<Constants.Sound.Sfx.Type, string> keyValuePair in Constants.Sound.Sfx.Paths)
        {
            AudioClip audioClip = await AddressablesAssetLoader.LoadAsync<AudioClip>(keyValuePair.Value);
            Clips.Add(keyValuePair.Key, audioClip);

            LoadedCount++;
        }
    }

    public void UnloadContent()
    {
        foreach (KeyValuePair<Constants.Sound.Sfx.Type, AudioClip> keyValuePair in Clips)
        {
            AddressablesAssetLoader.Unload(keyValuePair.Value);
            LoadedCount--;
        }
    }
}

#endregion

#region Shared

[Export(typeof(IAddressablesLoader))]
[ExportMetadata(nameof(AddressablesLoaderMetadata.OperableSceneNames), new[] {Constants.SceneNames.Menu, Constants.SceneNames.Desk})]
[ExportMetadata(nameof(AddressablesLoaderMetadata.IsExclusive), false)]
public class MusicAudioAddressablesLoader : IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => (uint)Constants.Sound.Music.Paths.Count;

    public List<AudioClip> Clips { get; } = new();

    public async Task LoadContent()
    {
        foreach (string path in Constants.Sound.Music.Paths)
        {
            Clips.Add(await AddressablesAssetLoader.LoadAsync<AudioClip>(path));
            LoadedCount++;
        }
    }

    public void UnloadContent()
    {
        foreach (AudioClip audioClip in Clips)
        {
            AddressablesAssetLoader.Unload(audioClip);
            LoadedCount--;
        }
    }
}

[Export(typeof(IAddressablesLoader))]
[ExportMetadata(nameof(AddressablesLoaderMetadata.OperableSceneNames), new[] {Constants.SceneNames.Menu, Constants.SceneNames.Desk})]
[ExportMetadata(nameof(AddressablesLoaderMetadata.IsExclusive), true)]
public class BackgroundImageAddressablesLoader : IAddressablesLoader
{   
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 1;
    
    public Sprite Sprite { get; private set; }

    public async Task LoadContent()
    {
        string id = SceneManager.GetActiveScene().name switch
        {
            Constants.SceneNames.Menu => Constants.Sprites.MenuBackground,
            Constants.SceneNames.Desk => Constants.Sprites.DeskBackground,
            _ => throw new NotImplementedException()
        };

        Sprite = await AddressablesAssetLoader.LoadAsync<Sprite>(id);

        LoadedCount = 1;
    }

    public void UnloadContent()
    {
        AddressablesAssetLoader.Unload(Sprite);
        LoadedCount = 0;
    }
}

[Export(typeof(IAddressablesLoader))]
[ExportMetadata(nameof(AddressablesLoaderMetadata.OperableSceneNames), new[] {Constants.SceneNames.Menu, Constants.SceneNames.Desk})]
[ExportMetadata(nameof(AddressablesLoaderMetadata.IsExclusive), false)]
public class SdtConnectionResultAddressablesLoader : IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 5;

    public SdtConnectionResultUI.Sprites Sprites { get; private set; }
    
    private Sprite _disconnectedSprite;
    private Sprite _loadingSprite;
    private Sprite _successSprite;
    private Sprite _failSprite;
    private Sprite _abandonedSprite;

    public async Task LoadContent()
    {
        _disconnectedSprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Disconnected);
        _loadingSprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Loading);
        _successSprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Success);
        _failSprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Fail);
        _abandonedSprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Abandoned);

        Sprites = new SdtConnectionResultUI.Sprites(
            _disconnectedSprite, _loadingSprite, _successSprite, _failSprite, _abandonedSprite
            );

        LoadedCount += 5;
    }

    public void UnloadContent()
    {
        AddressablesAssetLoader.Unload(_disconnectedSprite);
        AddressablesAssetLoader.Unload(_loadingSprite);
        AddressablesAssetLoader.Unload(_successSprite);
        AddressablesAssetLoader.Unload(_failSprite);
        AddressablesAssetLoader.Unload(_abandonedSprite);

        LoadedCount = 0;
    }
}

[Export(typeof(IAddressablesLoader))]
[ExportMetadata(nameof(AddressablesLoaderMetadata.OperableSceneNames), new[] {Constants.SceneNames.Menu, Constants.SceneNames.Desk})]
[ExportMetadata(nameof(AddressablesLoaderMetadata.IsExclusive), false)]
public class SoundUIAddressablesLoader : IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 2;

    public Sprite MusicSprite { get; private set; }
    public Sprite CrossSprite { get; private set; }

    public async Task LoadContent()
    {
        Sprite musicSprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.Music);
        LoadedCount++;
        
        Sprite crossSprite = await AddressablesAssetLoader.LoadAsync<Sprite>(Constants.Sprites.Cross);
        LoadedCount++;
        
        MusicSprite = musicSprite;
        CrossSprite = crossSprite;
    }

    public void UnloadContent()
    {
        AddressablesAssetLoader.Unload(MusicSprite);
        AddressablesAssetLoader.Unload(CrossSprite);

        LoadedCount = 0;
    }
}

/// <summary>
/// Loader of UI (scene specific) prefab from Addressables.
/// </summary>
[Export(typeof(IAddressablesLoader))]
[ExportMetadata(nameof(AddressablesLoaderMetadata.OperableSceneNames), new[] {Constants.SceneNames.Menu, Constants.SceneNames.Desk})]
[ExportMetadata(nameof(AddressablesLoaderMetadata.IsExclusive), true)]
public class UIAddressablesLoader : IAddressablesLoader
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 1; // Because we have only one 'Main' UI prefab to load.

    private GameObject _uiPrefab;

    public async Task LoadContent()
    {
        await InstantiateContent(GetPrefabId());
        
        LoadedCount++;
    }

    private async Task InstantiateContent(string prefabId)
    {
        _uiPrefab = await Addressables.InstantiateAsync(prefabId).Task;

        Logger.Log($"Instantiated asset: {_uiPrefab}", Logger.LogSource.AddressablesLoader);
    }
    
    public void UnloadContent()
    {
        AddressablesAssetLoader.UnloadInstance(_uiPrefab);
        LoadedCount = 0;
    }

    private string GetPrefabId()
    {
        string templateName = SceneManager.GetActiveScene().name switch
        {
            Constants.SceneNames.Menu => Constants.Prefabs.UI.Menu,
            Constants.SceneNames.Desk => Constants.Prefabs.UI.Desk,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (Application.isMobilePlatform == true)
        {
            templateName += Constants.Prefabs.MobileSuffix;
        }
        
        return templateName;
    }
}

#endregion