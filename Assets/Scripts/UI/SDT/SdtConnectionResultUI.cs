using System;
using System.Collections.Generic;
using SDT;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(SdtConnectionStatusHoverTooltip))]
public class SdtConnectionResultUI : MonoBehaviour
{
    public struct Sprites
    {
        public Sprite DisconnectedSprite { get; }
        public Sprite LoadingSprite { get; }
        public Sprite SuccessSprite { get; }
        public Sprite FailSprite { get; }
        public Sprite AbandonedSprite { get; }

        public Sprites(Sprite disconnectedSprite, Sprite loadingSprite, Sprite successSprite, Sprite failSprite, Sprite abandonedSprite)
        {
            DisconnectedSprite = disconnectedSprite;
            LoadingSprite = loadingSprite;
            SuccessSprite = successSprite;
            FailSprite = failSprite;
            AbandonedSprite = abandonedSprite;
        }
    }
    
    public static Server SdtServer => Server.Instance;
    public static Client SdtClient => Client.Instance;

    public SdtType SdtType => _sdtType;
    [SerializeField] private SdtType _sdtType;
    
    [SerializeField] private Image _image;

    private SdtConnectionStatusHoverTooltip _hoverTooltip;

    private Animator _animator;
    
    private static readonly int Loading = Animator.StringToHash("Loading");

    private Sprites _sprites;

    private void Awake()
    {
        // Shouldn`t be on the client in Desk scene.
        if (NetworkManager.Singleton.IsServer == false && _sdtType == SdtType.Server)
        {
            Destroy(gameObject);
            return;
        }
        
        // Unity Relay is not supported by SDT.
        if (NetworkConnectorHandler.State != NetworkConnectorHandler.ConnectionState.Disconnected && 
            NetworkConnectorFactory.GetEnumType(NetworkConnectorHandler.Connector) is NetworkConnectorType.UnityRelay)
        {
            Destroy(gameObject);
            return;
        }
        
        _hoverTooltip = GetComponent<SdtConnectionStatusHoverTooltip>();
        _hoverTooltip.SetSdtType(_sdtType);

        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        switch (_sdtType)
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

    public void SetSprites(Sprites sprites)
    {
        _sprites = sprites;
        
        switch (_sdtType)
        {
            case SdtType.Server:
                OnSdtConnectionStateChanged(SdtServer.ConnectionState);
                break;
            case SdtType.Client:
                OnSdtConnectionStateChanged(SdtClient.ConnectionState);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnSdtConnectionStateChanged(ConnectionState connectionState)
    {
        _animator.SetBool(Loading, connectionState == ConnectionState.Connecting);

        switch (connectionState)
        {
            case ConnectionState.Connecting:
                _image.sprite = _sprites.LoadingSprite;
                break;
            case ConnectionState.Successful:
                _image.sprite = _sprites.SuccessSprite;
                break;
            case ConnectionState.Failed:
                _image.sprite = _sprites.FailSprite;
                break;
            case ConnectionState.Disconnected:
            case ConnectionState.DisconnectedPortClosed:
                _image.sprite = _sprites.DisconnectedSprite;
                break;
            case ConnectionState.Abandoned:
                _image.sprite = _sprites.AbandonedSprite;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, null);
        }
        
        _hoverTooltip.SetupText();
    }

    // Button.
    public void Reconnect()
    {
        ConnectionState state;
        
        switch (_sdtType)
        {
            case SdtType.Server:
                if (SdtServer == null) return;
                
                state = SdtServer.ConnectionState;
                break;
            case SdtType.Client:
                if (SdtClient == null) return;

                state = SdtClient.ConnectionState;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (state is ConnectionState.Successful or ConnectionState.Connecting or ConnectionState.Abandoned)
        {
            return;
        }

        switch (_sdtType)
        {
            case SdtType.Server:
                SdtServer.Reconnect();
                break;
            case SdtType.Client:
                SdtClient.Reconnect();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    } 
}
