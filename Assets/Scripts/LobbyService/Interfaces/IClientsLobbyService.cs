using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.Interfaces
{
    /// <summary>
    /// <para> You must be a CLIENT and use it ONLY in a standalone builds. </para> 
    /// </summary>
    public interface IClientsLobbyService
    {
        event Action<ConnectionState> ConnectionStateChangedEvent;
        
        ConnectionState ConnectionState { get; }
        
        Task Start();
        Task Stop();
        
        Task<List<LobbyDto>> GetLobbiesInfoAsync(CancellationToken cancellationToken = default);
        Task<List<Guid>> GetLobbiesGuidsAsync(CancellationToken cancellationToken = default);
    }
}