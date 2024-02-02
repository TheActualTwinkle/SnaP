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
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => 5;

    private SdtConnectionResultUI _sdtConnectionResultUi;

    private static Client SdtClient => SdtConnectionResultUI.SdtClient;
    private static Server SdtServer => SdtConnectionResultUI.SdtServer;

    private Sprite _disconnectedSprite;
    private Sprite _loadingSprite;
    private Sprite _successSprite;
    private Sprite _failSprite;
    private Sprite _abandonedSprite;
    
    private async void Awake()
    {
        _sdtConnectionResultUi = GetComponent<SdtConnectionResultUI>();
        await LoadContent();
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
    
    public async Task LoadContent()
    {
        _disconnectedSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Disconnected);
        _loadingSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Loading);
        _successSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Success);
        _failSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Fail);
        _abandonedSprite = await AddressablesLoader.LoadAsync<Sprite>(Constants.Sprites.Sdt.Abandoned);

        LoadedCount += 5;
    }

    public void UnloadContent()
    {
        AddressablesLoader.Unload(_disconnectedSprite);
        AddressablesLoader.Unload(_loadingSprite);
        AddressablesLoader.Unload(_successSprite);
        AddressablesLoader.Unload(_failSprite);
        AddressablesLoader.Unload(_abandonedSprite);

        LoadedCount = 0;
    }
    
    private async void OnSdtConnectionStateChanged(ConnectionState connectionState)
    {
        if (LoadedCount == 0)
        {
            await WaitUntilLoaded();
        }
        
        Sprite sprite;
        
        switch (connectionState)
        {
            case ConnectionState.Connecting:
                sprite = _loadingSprite;
                break;
            case ConnectionState.Successful:
                sprite = _successSprite;
                break;
            case ConnectionState.Failed:
                sprite = _failSprite;
                break;
            case ConnectionState.Disconnected:
            case ConnectionState.DisconnectedPortClosed:
                sprite = _disconnectedSprite;
                break;
            case ConnectionState.Abandoned:
                sprite = _abandonedSprite;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, null);
        }
        
        _sdtConnectionResultUi.SetSprite(sprite);
    }
    
    private async Task WaitUntilLoaded()
    {
        while (LoadedCount == 0)
        {
            await Task.Yield();
        }
    }
}
