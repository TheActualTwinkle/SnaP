using System.Collections.Generic;

public interface INetworkConnector
{
    public IEnumerable<string> ConnectionData { get; }
    
    public void CreateGame();
    public void JoinGame();
}
