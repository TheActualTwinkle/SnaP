using System;
using LobbyService;
using LobbyService.Interfaces;
using LobbyService.TcpIp;
using Zenject;

public class LobbyServiceConnectionStatusHoverTooltip : HoverTooltip
{
    private IServersLobbyService _serversLobbyService;
    private IClientsLobbyService _clientsLobbyService;

    private LobbyServiceType _lobbyServiceType;

    [Inject]
    private void Construct(IServersLobbyService serversLobbyService, IClientsLobbyService clientsLobbyService)
    {
        _serversLobbyService = serversLobbyService;
        _clientsLobbyService = clientsLobbyService;
    }
    
    public void SetSdtType(LobbyServiceType lobbyServiceType)
    {
        _lobbyServiceType = lobbyServiceType;
    }
    
    public override void SetupText()
    {
        ConnectionState state = _lobbyServiceType switch
        {
            LobbyServiceType.Server => _serversLobbyService.ConnectionState,
            LobbyServiceType.Client => _clientsLobbyService.ConnectionState,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        Text.text = state switch
        {
            ConnectionState.Connecting => "Connecting to SnaP Data Transfer server...",
            ConnectionState.Successful => "You are connected to SnaP Data Transfer server!",
            ConnectionState.Failed => "Connection to SnaP Data Transfer server failed.\nClick to reconnect.",
            ConnectionState.Disconnected => "Disconnected (unknown reason).\nClick to reconnect.",
            ConnectionState.DisconnectedPortClosed => "Disconnected.\nYour lobby is not visible.\nClick to reconnect.",
            _ => throw new ArgumentOutOfRangeException()
        };

    }
}
