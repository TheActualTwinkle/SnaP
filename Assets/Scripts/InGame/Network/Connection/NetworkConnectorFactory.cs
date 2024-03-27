using System;

public static class NetworkConnectorFactory
{
    /// <summary>
    /// Get the network connector by the enum type.
    /// </summary>
    /// <param name="connectorType">The enum type of the network connector.</param>
    /// <returns>The network connector.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static INetworkConnector Get(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = connectorType switch
        {
            NetworkConnectorType.IpAddress => new IPAddressNetworkConnector(),
            NetworkConnectorType.UnityRelay => new UnityRelayNetworkConnector(),
            NetworkConnectorType.IpAddressDedicatedServer => new DedicatedServerNetworkConnector(),
            NetworkConnectorType.UPnP => new UPnPNetworkConnector(),
            _ => throw new ArgumentOutOfRangeException(nameof(connectorType), connectorType, null)
        };
        
        return connector;
    }

    /// <summary>
    /// Get the enum type of the network connector.
    /// </summary>
    /// <param name="connector">The network connector.</param>
    /// <returns>The enum type of the network connector.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static NetworkConnectorType GetEnumType(INetworkConnector connector)
    {
        return connector switch
        {
            IPAddressNetworkConnector _ => NetworkConnectorType.IpAddress,
            UnityRelayNetworkConnector _ => NetworkConnectorType.UnityRelay,
            DedicatedServerNetworkConnector _ => NetworkConnectorType.IpAddressDedicatedServer,
            UPnPNetworkConnector _ => NetworkConnectorType.UPnP,
            _ => throw new ArgumentOutOfRangeException(nameof(connector))
        };
    }
}
