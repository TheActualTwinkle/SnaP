using System;
using System.Threading.Tasks;
using Unity.Netcode;

public static class NetworkConnectorHandler
{
    public const uint MaxPlayersAmount = 5;
    
    public static INetworkConnector CurrentConnector { get; private set; }

    private static bool _isSubscribedToUserConnectionEvents; 
    
    public static async Task CreateGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        
        CurrentConnector = connector;
        await connector.Init();

        Logger.Log($"====== STARTING AT: {string.Join(':', connector.ConnectionData)} ======");

        if (await connector.TryCreateGame() == false)
        {
            Logger.Log("Fail to start at " + string.Join(':', connector.ConnectionData), Logger.Level.Error);
        }
        else
        {
            Logger.Log("Successfully started at " + string.Join(':', connector.ConnectionData));
        }

        if (_isSubscribedToUserConnectionEvents == false)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            _isSubscribedToUserConnectionEvents = true;
        }
    }

    public static async Task JoinGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        
        CurrentConnector = connector;
        await connector.Init();

        Logger.Log($"==== JOINING TO: {string.Join(':', connector.ConnectionData)} ====");

        if (await connector.TryJoinGame() == false)
        {
            Logger.Log("Failed to join to " + string.Join(':', connector.ConnectionData), Logger.Level.Error);
        }
        
        NetworkManager.Singleton.NetworkConfig.NetworkTransport.OnTransportEvent += OnNetworkTransportEvent;
    }

    // For clients only.
    private static void OnNetworkTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
    {
        switch (eventType)
        {
            case NetworkEvent.Connect:
                Logger.Log($"Successfully connected to {string.Join(':', CurrentConnector.ConnectionData)}");
                break;
            case NetworkEvent.Disconnect:
                Logger.Log($"Failed to connect to {string.Join(':', CurrentConnector.ConnectionData)}", Logger.Level.Error);
                break;
        }
        
        NetworkManager.Singleton.NetworkConfig.NetworkTransport.OnTransportEvent -= OnNetworkTransportEvent;
    }

    private static void OnClientConnected(ulong id)
    {
        Logger.Log($"User connected. ID: '{id}'");
    }

    private static void OnClientDisconnected(ulong id)
    {
        Logger.Log($"User disconnected. ID: '{id}'");
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
