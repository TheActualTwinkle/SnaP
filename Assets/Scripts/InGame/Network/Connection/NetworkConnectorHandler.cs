using System;

public static class NetworkConnectorHandler
{
    public const uint MaxPlayersAmount = 5;
    
    public static INetworkConnector CurrentConnector { get; private set; }

    public static void CreateGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        
        CurrentConnector = connector;
        connector.Init();

        connector.CreateGame();
    }

    public static void JoinGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        
        CurrentConnector = connector;
        connector.Init();

        connector.JoinGame();
    }

    private static INetworkConnector GetConnector(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = connectorType switch
        {
            NetworkConnectorType.LocalAddress => new LocalAddressNetworkConnector(),
            NetworkConnectorType.UnityRelay => new UnityRelayNetworkConnector(),
            NetworkConnectorType.DedicatedServer => new DedicatedServerNetworkConnector(),
            _ => throw new ArgumentOutOfRangeException(nameof(connectorType), connectorType, null)
        };
        
        return connector;
    }
}
