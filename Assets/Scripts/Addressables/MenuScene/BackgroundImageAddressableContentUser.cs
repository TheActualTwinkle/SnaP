using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackgroundImageAddressableContentUser : MonoBehaviour, IAddressableContentUser
{    
    [SerializeField] private Image _backgroundImage;

    private Sprite _loadedSprite;
    
    private void Start()
    {
        LoadContent();
    }

    private void OnDestroy()
    {
        AddressablesLoader.Unload(_loadedSprite);
    }

    public async void LoadContent()
    {
        string id = SceneManager.GetActiveScene().name switch
        {
            Constants.SceneNames.Menu => Constants.Sprites.MenuBackgroundSprite,
            Constants.SceneNames.Desk => Constants.Sprites.DeskBackgroundSprite,
            _ => throw new NotImplementedException()
        };

        _loadedSprite = await AddressablesLoader.LoadAsync<Sprite>(id);
        _backgroundImage.sprite = _loadedSprite;
    }
}
