using System.Collections.Generic;
using System.Threading.Tasks;

public interface INetworkConnector
{
    public IEnumerable<string> ConnectionData { get; }

    Task Init();
    public Task<bool> TryCreateGame();
    public Task<bool> TryJoinGame();
}
