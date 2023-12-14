using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyAndCode.UI;
using SDT;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListCell : MonoBehaviour, ICell
{
    public static LobbyInfo SelectedLobbyInfo { get; private set; }

    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _lobbyPlayersCountText; // e.g "2/5"

    [Range(1f, 5f)]
    [SerializeField] private float _joinButtonDisabledTimeSeconds;
    [SerializeField] private Button _joinButton;
    [SerializeField] private TextMeshProUGUI _buttonText;
    
    private IEnumerator _joinCoroutine;
    
    private LobbyInfo _lobbyInfo;
    
    private static bool _isJoining;

    public void SetLobbyInfo(LobbyInfo lobbyInfo, int index)
    {
        _lobbyInfo = lobbyInfo;
        
        _lobbyNameText.text = lobbyInfo.LobbyName;
        _lobbyPlayersCountText.text = $"{lobbyInfo.PlayersCount}/{lobbyInfo.MaxSeats}";
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
        SelectedLobbyInfo = _lobbyInfo;
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
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        await Task.Delay((int)(_joinButtonDisabledTimeSeconds * 1000));
        
        _buttonText.text = "Join";
        
        _isJoining = false;
        _joinButton.interactable = true;

        NetworkConnectorHandler.ConnectionStateChangedEvent -= OnConnectionStateChanged;
    }
}
