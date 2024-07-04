using System;
using System.Collections;
using System.Threading.Tasks;
using PolyAndCode.UI;
using LobbyService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListCell : MonoBehaviour, ICell
{
    public static LobbyDto SelectedLobbyDto { get; private set; }

    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _lobbyPlayersCountText; // e.g "2/5"

    [Range(1f, 5f)]
    [SerializeField] private float _joinButtonDisabledTimeSeconds;
    [SerializeField] private Button _joinButton;
    [SerializeField] private TextMeshProUGUI _buttonText;
    
    private IEnumerator _joinCoroutine;
    
    private LobbyDto _lobbyDto;
    
    private static bool _isJoining;

    private void Start()
    {
        _isJoining = false;
    }

    public void SetLobbyInfo(LobbyDto lobbyDto, int index)
    {
        if (lobbyDto == null)
        {
            Logger.Log("LobbyInfo is null!", Logger.LogLevel.Error);
            return;
        }
        
        _lobbyDto = lobbyDto;
        
        _lobbyNameText.text = lobbyDto.LobbyName;
        _lobbyPlayersCountText.text = $"{lobbyDto.PlayersCount}/{lobbyDto.MaxSeats}";
    }

    // Button.
    private void StartJoining()
    {
        if (_isJoining == true)
        {
            return;
        }

        SelectLobby();
        Join();
    }

    private void SelectLobby()
    {
        SelectedLobbyDto = _lobbyDto;
    }
    
    private async void Join()
    {
        _isJoining = true;
        
        _joinButton.interactable = false;
        _buttonText.text = "Connecting...";

        NetworkConnectorHandler.ConnectionStateChangedEvent += OnConnectionStateChanged;
        
        await NetworkConnectorHandler.JoinGame(NetworkConnectorType.UPnP);
    }

    private async void OnConnectionStateChanged(NetworkConnectorHandler.ConnectionState state)
    {
        switch (state)
        {
            case NetworkConnectorHandler.ConnectionState.Canceled:
                _buttonText.text = "Canceled";
                break;
            case NetworkConnectorHandler.ConnectionState.Connecting:
                _buttonText.text = "Connecting...";
                break;
            case NetworkConnectorHandler.ConnectionState.Successful:
                // If connect is OK we don't need to do anything. Changing scene!
                break;
            case NetworkConnectorHandler.ConnectionState.Failed:
                _buttonText.text = "Connect failed";
                break;
            case NetworkConnectorHandler.ConnectionState.Disconnected:
                _buttonText.text = "Disconnected";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        await Task.Delay(TimeSpan.FromSeconds(_joinButtonDisabledTimeSeconds));

        if (_buttonText == null || _joinButton == null)
        {
            return;
        }
        
        _buttonText.text = "Join";
        
        _isJoining = false;
        _joinButton.interactable = true;

        NetworkConnectorHandler.ConnectionStateChangedEvent -= OnConnectionStateChanged;
    }
}
