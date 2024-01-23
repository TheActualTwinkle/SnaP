using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDT;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SdtConnectionResultUI))]
public class SdtConnectionResultAddressableContentUser : MonoBehaviour, IAddressableContentUser
{
    private SdtConnectionResultUI _sdtConnectionResultUi;

    private static Client SdtClient => SdtConnectionResultUI.SdtClient;
    private static Server SdtServer => SdtConnectionResultUI.SdtServer;
    
    [SerializeField] private Image _image;

    private Sprite _disconnectedSprite;
    private Sprite _loadingSprite;
    private Sprite _successSprite;
    private Sprite _failSprite;
    private Sprite _abandonedSprite;

    private static bool IsLoaded;
    
    private void Awake()
    {
        _sdtConnectionResultUi = GetComponent<SdtConnectionResultUI>();
        LoadContent();
    }

    private void OnEnable()
    {
        switch (_sdtConnectionResultUi.SdtType)
        {
            case SdtType.Server:
                SdtServer.ConnectionStateChangedEvent += OnSdtConnectionStateChanged;
                break;
            case SdtType.Client:
                SdtClient.ConnectionStateChangedEvent += OnSdtConnectionStateChanged;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnDisable()
    {
        if (SdtServer != null) SdtServer.ConnectionStateChangedEvent -= OnSdtConnectionStateChanged;
        if (SdtClient != null) SdtClient.ConnectionStateChangedEvent -= OnSdtConnectionStateChanged;
    }
    
    private void OnDestroy()
    {
        UnloadContent();
    }

    public async void LoadContent()
    {
        _disconnectedSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Disconnected);
        _loadingSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Loading);
        _successSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Success);
        _failSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Fail);
        _abandonedSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Abandoned);

        IsLoaded = true;
    }

    public void UnloadContent()
    {
        AddressablesLoader.Unload(_disconnectedSprite);
        AddressablesLoader.Unload(_loadingSprite);
        AddressablesLoader.Unload(_successSprite);
        AddressablesLoader.Unload(_failSprite);
        AddressablesLoader.Unload(_abandonedSprite);

        IsLoaded = false;
    }
    
    private async void OnSdtConnectionStateChanged(ConnectionState connectionState)
    {
        if (IsLoaded == false)
        {
            await WaitUntilLoaded();
        }
        
        switch (connectionState)
        {
            case ConnectionState.Connecting:
                _image.sprite = _loadingSprite;
                break;
            case ConnectionState.Successful:
                _image.sprite = _successSprite;
                break;
            case ConnectionState.Failed:
                _image.sprite = _failSprite;
                break;
            case ConnectionState.Disconnected:
            case ConnectionState.DisconnectedPortClosed:
                _image.sprite = _disconnectedSprite;
                break;
            case ConnectionState.Abandoned:
                _image.sprite = _abandonedSprite;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, null);
        }
    }
    
    private static async Task WaitUntilLoaded()
    {
        while (IsLoaded == false)
        {
            await Task.Yield();
        }
    }
}
