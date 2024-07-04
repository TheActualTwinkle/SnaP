using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LobbyService.Interfaces;
using LobbyService.TcpIp.Commands;
using LobbyService.TcpIp.Factories;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace LobbyService.TcpIp
{
    /// <summary>
    /// <para> This must be used ONLY if you are SnaP SERVER. </para> 
    /// </summary>
    public class ServersTcpIpLobbyService : IServersLobbyService
    {
        public event Action<ConnectionState> ConnectionStateChangedEvent;
        
        public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;
        
        private NetworkStream NetworkStream => _tcpClient.GetStream();
        private TcpClient _tcpClient;
        
        private IPEndPoint IpEndPoint => _lobbyServiceInfo.ServersEndPoint;
        
        private readonly LobbyServiceInfo _lobbyServiceInfo;

        public ServersTcpIpLobbyService(LobbyServiceInfo lobbyServiceInfo)
        {
            _lobbyServiceInfo = lobbyServiceInfo;
        }
        
        public async Task Start()
        {
            // If this is a client
            if (NetworkManager.Singleton.IsServer == false)
            {
                SetConnectionState(ConnectionState.Failed);
                throw new UnityException("ServersTcpIpLobbyService must be used ONLY if you are SnaP SERVER.");
            }
            
            UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

            // We cant use SDT with unity relay.
            if (unityTransport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
            {
                SetConnectionState(ConnectionState.Failed);
                throw new UnityException("We cant use ServersTcpIpLobbyService with unity relay.");
            }

            await ConnectAsync();
        }

        public async Task Stop()
        {
            await DisconnectAsync();
        }

        public async Task PostLobbyData(LobbyDto lobbyDto, CancellationToken cancellationToken = default)
        {
            string data = JsonConvert.SerializeObject(lobbyDto);
            await CommandFactory.StartCommandAsync(CommandType.PostLobbyInfo, data, NetworkStream, cancellationToken);
        }

        public async Task DropLobby(CancellationToken cancellationToken = default)
        {
            await CommandFactory.StartCommandAsync(CommandType.Close, default, NetworkStream, cancellationToken);
        }

        private async Task ConnectAsync()
        {
            SetConnectionState(ConnectionState.Connecting);
            
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(IpEndPoint.Address, IpEndPoint.Port);
            }
            catch (Exception e)
            {
                SetConnectionState(ConnectionState.Failed);
                
                Logger.Log($"Can`t connect to {IpEndPoint.Address}:{IpEndPoint.Port}. {e}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
                throw;
            }
            
            SetConnectionState(ConnectionState.Successful);
            
            Logger.Log($"Connected to server at {IpEndPoint.Address}:{IpEndPoint.Port}.", Logger.LogSource.LobbyService);

            try
            {
                await WaitUntilLobbyInitialized();
                
                ushort port = ConnectionDataPresenter.GetGamePort();
                
                await ExecuteCommandByContext(port);

                PlayerSeats.Instance.PlayerSitEvent += async (_, _) => { await ExecuteCommandByContext(port); };            
            
                PlayerSeats.Instance.PlayerLeaveEvent += async (_, _) => { await ExecuteCommandByContext(port); };            
            }
            catch (Exception e)
            {
                SetConnectionState(ConnectionState.Failed);
                
                Logger.Log($"Exception while sending data to server. {e}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
            }
        }
        
        private async Task DisconnectAsync()
        {
            SetConnectionState(ConnectionState.Disconnected);
            
            if (_tcpClient is { Connected: true })
            {
                await CommandFactory.StartCommandAsync(CommandType.Close, default, NetworkStream, Constants.LobbyService.CreateTimeoutToken());
            }

            _tcpClient?.Close();
        }

        // Sending some message based on Game Context/Connection situation.
        private async Task ExecuteCommandByContext(ushort port)
        {
            if (await UdpPortChecker.IsForwarded(port) == false)
            {
                await DisconnectAsync();
                return;
            }

            if (_tcpClient is not { Connected: true })
            {
                return;
            }
            
            if (ConnectionState == ConnectionState.Disconnected)
            {
                await DropLobby();
            }

            LobbyDto lobbyDto = new(string.Empty, 0, 0, 0, "Awaiting lobby initialization...");
            if (IsLobbyInitialized() == false)
            {
                await PostLobbyData(lobbyDto, Constants.LobbyService.CreateTimeoutToken());
                return;
            }
            
            IPAddress ipAddress = await ConnectionDataPresenter.GetOrUpdatePublicIpAddressAsync();
            ushort lobbyPort = ConnectionDataPresenter.GetGamePort();

            lobbyDto = new LobbyDto(
                ipAddress.ToString(),
                lobbyPort,
                PlayerSeats.MaxSeats,
                PlayerSeats.Instance.PlayersAmount + PlayerSeats.Instance.WaitingPlayersAmount,
                "Best Lobby!" // TODO: Create a lobby name feature!
            );
            
            await PostLobbyData(lobbyDto, Constants.LobbyService.CreateTimeoutToken());
        }
        
        private async Task WaitUntilLobbyInitialized()
        {
            while (true)
            {
                if (IsLobbyInitialized() == true)
                {
                    break;
                }
                
                await Task.Delay(_lobbyServiceInfo.ServerLobbyInitializationInterval);
            }
        }
        
        private static bool IsLobbyInitialized()
        {
            return PlayerSeats.Instance != null;
        }
        
        private void SetConnectionState(ConnectionState state)
        {
            ConnectionState = state;
            ConnectionStateChangedEvent?.Invoke(state);
        }
    }   
}