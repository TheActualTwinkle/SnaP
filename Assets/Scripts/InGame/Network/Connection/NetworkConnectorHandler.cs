using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public static class NetworkConnectorHandler
{
    public const uint MaxPlayersAmount = 5;
    
    public static INetworkConnector CurrentConnector { get; private set; }
    
    private static ConnectionInputFields ConnectionInputFields => ConnectionInputFields.Instance;
    
    public static void CreateGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        connector.CreateGame();
    }

    public static void JoinGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        connector.JoinGame();
    }

    private static INetworkConnector GetConnector(NetworkConnectorType connectorType)
    {
        IReadOnlyList<string> connectionData = ConnectionInputFields.GetConnectionData(connectorType);
        
        INetworkConnector connector = connectorType switch
        {
            NetworkConnectorType.LocalAddress => new LocalAddressNetworkConnector(connectionData),
            NetworkConnectorType.UnityRelay => new UnityRelayNetworkConnector(connectionData),
            _ => throw new ArgumentOutOfRangeException(nameof(connectorType), connectorType, null)
        };

        CurrentConnector = connector;
        
        return connector;
    }
}
