using System;
using System.Collections.Generic;

public static class NetworkConnectorHandler
{
    public const uint MaxPlayersAmount = 5;
    
    public static INetworkConnector CurrentConnector { get; private set; }
    
    private static ConnectionInputField ConnectionInputField => ConnectionInputField.Instance;
    
    public static void CreateGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        
        CurrentConnector = connector;

        connector.CreateGame();
    }

    public static void JoinGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        
        CurrentConnector = connector;

        connector.JoinGame();
    }

    private static INetworkConnector GetConnector(NetworkConnectorType connectorType)
    {
        IReadOnlyList<string> connectionData = ConnectionInputField.GetConnectionData(connectorType);
        
        INetworkConnector connector = connectorType switch
        {
            NetworkConnectorType.LocalAddress => new LocalAddressNetworkConnector(connectionData),
            NetworkConnectorType.UnityRelay => new UnityRelayNetworkConnector(connectionData),
            _ => throw new ArgumentOutOfRangeException(nameof(connectorType), connectorType, null)
        };
        
        return connector;
    }
}
