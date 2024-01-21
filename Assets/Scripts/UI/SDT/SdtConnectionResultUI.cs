using System;
using SDT;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(SdtConnectionStatusHoverTooltip))]
public class SdtConnectionResultUI : MonoBehaviour
{
    private static Client SdtClient => Client.Instance;
    private static Server SdtServer => Server.Instance;

    [SerializeField] private SdtType _sdtType;
    
    [SerializeField] private Image _image;

    [SerializeField] private Sprite _disconnectedSprite;
    [SerializeField] private Sprite _loadingSprite;
    [SerializeField] private Sprite _successSprite;
    [SerializeField] private Sprite _failSprite;
    [SerializeField] private Sprite _abandonedSprite;

    private SdtConnectionStatusHoverTooltip _hoverTooltip;

    private Animator _animator;
    
    private static readonly int Loading = Animator.StringToHash("Loading");

    private void Awake()
    {
        if (NetworkManager.Singleton.IsServer == false && _sdtType == SdtType.Server)
        {
            print("SdtConnectionResultUI should only be on the server.");
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

    private void OnSdtConnectionStateChanged(ConnectionState connectionState)
    {
        switch (connectionState)
        {
            case ConnectionState.Connecting:
                _image.sprite = _loadingSprite;
                _animator.SetBool(Loading, true);
                break;
            case ConnectionState.Successful:
                _image.sprite = _successSprite;
                _animator.SetBool(Loading, false);
                break;
            case ConnectionState.Failed:
                _image.sprite = _failSprite;
                _animator.SetBool(Loading, false);
                break;
            case ConnectionState.Disconnected:
            case ConnectionState.DisconnectedPortClosed:
                _image.sprite = _disconnectedSprite;
                _animator.SetBool(Loading, false);
                break;
            case ConnectionState.Abandoned:
                _image.sprite = _abandonedSprite;
                _animator.SetBool(Loading, false);
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
