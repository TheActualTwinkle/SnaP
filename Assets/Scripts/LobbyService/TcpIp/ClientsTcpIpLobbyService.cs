using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using LobbyService.Interfaces;
using LobbyService.TcpIp.Commands;
using LobbyService.TcpIp.Factories;
using UnityEngine;
using Newtonsoft.Json;

namespace LobbyService.TcpIp
{
    public class ClientsTcpIpLobbyService : IClientsLobbyService
    {
        public event Action<ConnectionState> ConnectionStateChangedEvent;
        
        public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;
        
        private NetworkStream NetworkStream => _tcpClient.GetStream();
        private TcpClient _tcpClient;
        
        private IPEndPoint IpEndPoint => _lobbyServiceInfo.ClientsEndPoint;

        private readonly LobbyServiceInfo _lobbyServiceInfo;

        public ClientsTcpIpLobbyService(LobbyServiceInfo lobbyServiceInfo)
        {
            _lobbyServiceInfo = lobbyServiceInfo;
        }

        public async Task Start()
        {
            await ConnectAsync();
        }
        
        public async Task Stop()
        {
            await DisconnectAsync();
        }
        
        private async Task ConnectAsync()
        {
            SetConnectionState(ConnectionState.Connecting);
            
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(IpEndPoint.Address, IpEndPoint.Port);
            }
            catch (Exception e) // todo CHECK IF THE SERVER ERROR IS SAME WITH CLIENT ERROR.
            {
                SetConnectionState(ConnectionState.Failed);
                
                Logger.Log($"Can`t connect to {IpEndPoint.Address}:{IpEndPoint.Port}. {e}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
                return;
            }
            
            Logger.Log($"Connected to server at {IpEndPoint.Address}:{IpEndPoint.Port}.", Logger.LogSource.LobbyService);

            SetConnectionState(ConnectionState.Successful);
        }
        
        private async Task DisconnectAsync()
        {
            SetConnectionState(ConnectionState.Disconnected);
            
            if (_tcpClient is { Connected: true })
            {
                await CommandFactory.StartCommandAsync(CommandType.Close, default, NetworkStream, CancellationToken.None);
            }

            _tcpClient?.Close();
        }
        
        public async Task<List<LobbyDto>> GetLobbiesInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await GetLobbyInfoAsyncInternal(cancellationToken);
            }
            catch (Exception e) // todo CHECK IF THE SERVER ERROR IS SAME WITH CLIENT ERROR; to make different errors.
            {
                SetConnectionState(ConnectionState.Failed);
                
                Logger.Log($"Can`t get lobby info. {e}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
                return null;                
            }
        }

        private async Task<List<LobbyDto>> GetLobbyInfoAsyncInternal(CancellationToken token)
        {
            List<Guid> guids = await GetLobbiesGuidsAsync(token);

            Logger.Log($"Received {guids.Count} lobbies count.", Logger.LogSource.LobbyService);

            List<LobbyDto> lobbyInfos = new();
            // Using the lobbies guids, get all lobbies info. 
            for (var i = 0; i < guids.Count; i++)
            {
                if (token.IsCancellationRequested == true)
                {
                    return null;
                }

                try
                {
                    Result<string> result = await CommandFactory.StartCommandAsync(CommandType.GetLobbyInfo, guids[i], NetworkStream, token);

                    if (result.IsFailed == true)
                    {
                        Logger.Log($"Can`t get lobby info at {i}/{guids.Count - 1}. {result.Errors.First()}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
                        continue;
                    }
                    
                    Logger.Log($"Received: {result.Value}", Logger.LogSource.LobbyService);
                    LobbyDto lobbyDto = JsonConvert.DeserializeObject<LobbyDto>(result.Value);
                    lobbyInfos.Add(lobbyDto);
                }
                catch (Exception e)
                {
                    Logger.Log($"Can`t deserialize json to LobbyInfo at {i}/{guids.Count - 1}. {e}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
                }
            }

            return lobbyInfos;
        }

        public async Task<List<Guid>> GetLobbiesGuidsAsync(CancellationToken cancellationToken)
        {
            var lobbyGuidsJson = string.Empty;
            
            // Getting count of lobbies.
            try
            {
                Result<string> result = await CommandFactory.StartCommandAsync(CommandType.GetLobbyGuids, default, NetworkStream, cancellationToken);

                if (result.IsFailed == true)
                {
                    Logger.Log($"Can`t get lobbies guids. {result.Errors.First()}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
                }
                
                lobbyGuidsJson = result.Value;
                List<Guid> lobbyGuids = JsonConvert.DeserializeObject<List<Guid>>(lobbyGuidsJson);
                return lobbyGuids;
            }
            catch (Exception e)
            {
                Logger.Log($"Can`t deserialize lobbies guids from {lobbyGuidsJson}. {e.Message}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
                return new List<Guid>();
            }
        }
        
        private void SetConnectionState(ConnectionState state)
        {
            ConnectionState = state;
            ConnectionStateChangedEvent?.Invoke(state);
        }
    }
}