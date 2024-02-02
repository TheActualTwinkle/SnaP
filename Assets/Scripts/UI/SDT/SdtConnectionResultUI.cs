using System;
using SDT;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(SdtConnectionStatusHoverTooltip))]
public class SdtConnectionResultUI : MonoBehaviour
{
    public static Server SdtServer => Server.Instance;
    public static Client SdtClient => Client.Instance;

    public SdtType SdtType => _sdtType;
    [SerializeField] private SdtType _sdtType;
    
    [SerializeField] private Image _image;

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
        _animator.SetBool(Loading, connectionState == ConnectionState.Connecting);

        _hoverTooltip.SetupText();
    }

    public void SetSprite(Sprite sprite)
    {
        _image.sprite = sprite;
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
