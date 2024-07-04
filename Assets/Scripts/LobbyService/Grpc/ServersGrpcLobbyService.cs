using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using LobbyService.Interfaces;

namespace LobbyService.Grpc
{
    public class ServersGrpcLobbyService : IServersLobbyService
    {
        public ServersGrpcLobbyService(IPEndPoint ipEndPoint)
        {
            _ipEndPoint = ipEndPoint;
        }
        
        private readonly IPEndPoint _ipEndPoint;
        private ServersHandler.ServersHandlerClient _client;
        
        public event Action<ConnectionState> ConnectionStateChangedEvent;

        public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;

        public async Task Start()
        {
            SetConnectionState(ConnectionState.Connecting);

            GrpcChannel channel = GrpcChannel.ForAddress($"https://{_ipEndPoint}");
            _client = new ServersHandler.ServersHandlerClient(channel);
            
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            SetConnectionState(ConnectionState.Successful);
        }

        public Task Stop()
        {
            _client = null!;
            SetConnectionState(ConnectionState.Disconnected);
            
            return Task.CompletedTask;
        }

        public async Task PostLobbyData(LobbyDto lobbyDto, CancellationToken cancellationToken)
        {
            await _client.PostLobbyInfoAsync(new PostLobbyInfoRequest
            {
                PublicIpAddress = lobbyDto.PublicIpAddress,
                Port = lobbyDto.Port,
                MaxSeats = lobbyDto.MaxSeats,
                PlayersCount = lobbyDto.PlayersCount,
                LobbyName = lobbyDto.LobbyName
            }, cancellationToken: cancellationToken);
        }
        
        public async Task DropLobby(CancellationToken cancellationToken)
        {
            await _client.DropLobbyAsync(new Empty(), cancellationToken: cancellationToken);
        }
        
        private void SetConnectionState(ConnectionState state)
        {
            ConnectionState = state;
            ConnectionStateChangedEvent?.Invoke(state);
        }
    }
}