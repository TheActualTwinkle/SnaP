using System;
using LobbyService;
using LobbyService.Interfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(Animator), typeof(LobbyServiceConnectionStatusHoverTooltip))]
public class LobbyServiceConnectionResultUI : MonoBehaviour
{
    public struct Sprites
    {
        public Sprite DisconnectedSprite { get; }
        public Sprite LoadingSprite { get; }
        public Sprite SuccessSprite { get; }
        public Sprite FailSprite { get; }

        public Sprites(Sprite disconnectedSprite, Sprite loadingSprite, Sprite successSprite, Sprite failSprite)
        {
            DisconnectedSprite = disconnectedSprite;
            LoadingSprite = loadingSprite;
            SuccessSprite = successSprite;
            FailSprite = failSprite;
        }
    }

    public LobbyServiceType LobbyServiceType => _lobbyServiceType;
    [SerializeField] private LobbyServiceType _lobbyServiceType;
    
    [SerializeField] private Image _image;

    private LobbyServiceConnectionStatusHoverTooltip _hoverTooltip;
    
    private static readonly int Loading = Animator.StringToHash("Loading");

    private Animator _animator;
    private Sprites _sprites;
    
    private IServersLobbyService _serversLobbyService; 
    private IClientsLobbyService _clientsLobbyService;

    [Inject]
    private void Construct(IServersLobbyService serversLobbyService, IClientsLobbyService clientsLobbyService)
    {
        _serversLobbyService = serversLobbyService;
        _clientsLobbyService = clientsLobbyService;
    }
    
    private void Awake()
    {
        // Shouldn`t be on the client in Desk scene.
        if (NetworkManager.Singleton.IsServer == false && _lobbyServiceType == LobbyServiceType.Server)
        {
            // Logger.Log($"{gameObject} marked as Server but appeared when NetworkManager.Singleton.IsServer == false");
            Destroy(gameObject);
            return;
        }
        
        // Unity Relay is not supported by LobbyService.
        if (NetworkConnectorHandler.State != NetworkConnectorHandler.ConnectionState.Disconnected && 
            NetworkConnectorFactory.GetEnumType(NetworkConnectorHandler.Connector) is NetworkConnectorType.UnityRelay)
        {
            // Logger.Log("Unity Relay is not supported by LobbyService.");
            Destroy(gameObject);
            return;
        }
        
        _hoverTooltip = GetComponent<LobbyServiceConnectionStatusHoverTooltip>();
        _hoverTooltip.SetSdtType(_lobbyServiceType);

        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        switch (_lobbyServiceType)
        {
            case LobbyServiceType.Server:
                _serversLobbyService.ConnectionStateChangedEvent += OnLobbyServiceConnectionStateChanged;
                break;
            case LobbyServiceType.Client:
                _clientsLobbyService.ConnectionStateChangedEvent += OnLobbyServiceConnectionStateChanged;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnDisable()
    {
        if (_serversLobbyService != null) _serversLobbyService.ConnectionStateChangedEvent -= OnLobbyServiceConnectionStateChanged;
        if (_clientsLobbyService != null) _clientsLobbyService.ConnectionStateChangedEvent -= OnLobbyServiceConnectionStateChanged;
    }

    public void SetSprites(Sprites sprites)
    {
        _sprites = sprites;
        
        switch (_lobbyServiceType)
        {
            case LobbyServiceType.Server:
                OnLobbyServiceConnectionStateChanged(_serversLobbyService.ConnectionState);
                break;
            case LobbyServiceType.Client:
                OnLobbyServiceConnectionStateChanged(_clientsLobbyService.ConnectionState);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnLobbyServiceConnectionStateChanged(ConnectionState connectionState)
    {
        _animator.SetBool(Loading, connectionState == ConnectionState.Connecting);

        _image.sprite = connectionState switch
        {
            ConnectionState.Connecting => _sprites.LoadingSprite,
            ConnectionState.Successful => _sprites.SuccessSprite,
            ConnectionState.Failed => _sprites.FailSprite,
            ConnectionState.Disconnected or ConnectionState.DisconnectedPortClosed => _sprites.DisconnectedSprite,
            _ => throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, null)
        };

        _hoverTooltip.SetupText();
    }

    // Button.
    public void Reconnect()
    {
        ConnectionState state;
        
        switch (_lobbyServiceType)
        {
            case LobbyServiceType.Server:
                if (_serversLobbyService == null) return;
                state = _serversLobbyService.ConnectionState;
                break;
            case LobbyServiceType.Client:
                if (_clientsLobbyService == null) return;
                state = _clientsLobbyService.ConnectionState;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (state is ConnectionState.Successful or ConnectionState.Connecting)
        {
            return;
        }

        switch (_lobbyServiceType)
        {
            case LobbyServiceType.Server:
                _serversLobbyService.Stop();
                _serversLobbyService.Start();
                break;
            case LobbyServiceType.Client:
                _clientsLobbyService.Stop();
                _clientsLobbyService.Start();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    } 
}
