using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using LobbyService.Interfaces;

namespace LobbyService.Grpc
{
    public class ClientsGrpcLobbyService : IClientsLobbyService
    {
        public ClientsGrpcLobbyService(IPEndPoint ipEndPoint)
        {
            _ipEndPoint = ipEndPoint;
        }
        
        public event Action<ConnectionState> ConnectionStateChangedEvent;

        public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;
        
        private readonly IPEndPoint _ipEndPoint;
        private ClientsHandler.ClientsHandlerClient _client;
        
        public async Task Start()
        {
            SetConnectionState(ConnectionState.Connecting);
            
            GrpcChannel channel = GrpcChannel.ForAddress($"https://{_ipEndPoint}");
            _client = new ClientsHandler.ClientsHandlerClient(channel);
            
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            SetConnectionState(ConnectionState.Successful);
        }

        public Task Stop()
        {
            _client = null!;
            SetConnectionState(ConnectionState.Disconnected);
            
            return Task.CompletedTask;
        }

        public async Task<List<LobbyDto>> GetLobbiesInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                List<Guid> guids = await GetLobbiesGuidsAsync(cancellationToken);

                List<LobbyDto> lobbies = new();
                foreach (Guid guid in guids)
                {
                    GetLobbyInfoResponse response = await _client.GetLobbyInfoAsync(new GetLobbyInfoRequest { Guid = guid.ToString() }, cancellationToken: cancellationToken);
                    lobbies.Add(new LobbyDto(
                        response.PublicIpAddress,
                        (ushort)response.Port,
                        response.MaxSeats,
                        response.PlayersCount,
                        response.LobbyName
                    ));
                }

                return lobbies;
            }
            catch (Exception e)
            {
                Logger.Log($"Can`t parse GetLobbyInfoResponse to List<LobbyDto>. {e.Message}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
                SetConnectionState(ConnectionState.Failed);
                return null;
            }
        }

        public async Task<List<Guid>> GetLobbiesGuidsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                GetGuidsResponse response = await _client.GetGuidsAsync(new Empty(), cancellationToken: cancellationToken);
                return response.Guids.Select(Guid.Parse).ToList();
            }
            catch (Exception e)
            {
                Logger.Log($"Can`t parse GetGuidsResponse to List<Guid>. {e.Message}", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
                SetConnectionState(ConnectionState.Failed);
                return null;
            }
        }

        private void SetConnectionState(ConnectionState state)
        {
            ConnectionState = state;
            ConnectionStateChangedEvent?.Invoke(state);
        }
    }
}