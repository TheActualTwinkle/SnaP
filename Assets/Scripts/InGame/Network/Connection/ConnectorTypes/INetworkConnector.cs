using System.Collections.Generic;
using System.Threading.Tasks;

public interface INetworkConnector
{
    NetworkConnectorType Type { get; }

    Task Init();
    public Task<bool> TryCreateGame();
    public Task<bool> TryJoinGame();
}
