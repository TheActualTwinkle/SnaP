using System;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.Interfaces
{
    public interface IServersLobbyService
    {
        event Action<ConnectionState> ConnectionStateChangedEvent;
        
        ConnectionState ConnectionState { get; }
        
        Task Start();
        Task Stop();
        
        Task PostLobbyData(LobbyDto lobbyDto, CancellationToken cancellationToken = default);

        Task DropLobby(CancellationToken cancellationToken = default);
    }
}