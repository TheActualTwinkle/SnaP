using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackgroundImageAddressableContentUser : MonoBehaviour, IAddressableContentUser
{   
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 1;

    [SerializeField] private Image _backgroundImage;

    private Sprite _loadedSprite;
    
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
        string id = SceneManager.GetActiveScene().name switch
        {
            Constants.SceneNames.Menu => Constants.Sprites.MenuBackground,
            Constants.SceneNames.Desk => Constants.Sprites.DeskBackground,
            _ => throw new NotImplementedException()
        };

        _loadedSprite = await AddressablesLoader.LoadAsync<Sprite>(id);
        _backgroundImage.sprite = _loadedSprite;

        LoadedCount = 1;
    }

    public void UnloadContent()
    {
        AddressablesLoader.Unload(_loadedSprite);
        LoadedCount = 0;
    }
}
