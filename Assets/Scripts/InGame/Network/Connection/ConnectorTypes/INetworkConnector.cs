using System.Collections.Generic;

public interface INetworkConnector
{
    public IEnumerable<string> ConnectionData { get; }

    void Init();
    public void CreateGame();
    public void JoinGame();
}
