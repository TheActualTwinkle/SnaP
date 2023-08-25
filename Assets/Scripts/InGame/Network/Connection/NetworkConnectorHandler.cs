using System;
using System.Threading.Tasks;
using Unity.Netcode;

public static class NetworkConnectorHandler
{
    public const uint MaxPlayersAmount = 5;
    
    public static INetworkConnector CurrentConnector { get; private set; }

    public static async Task CreateGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        
        CurrentConnector = connector;
        await connector.Init();

        Log.WriteToFile($"====== STARTING AT: {string.Join(':', connector.ConnectionData)} ======");

        if (await connector.TryCreateGame() == false)
        {
            Log.WriteToFile("Error: fail to start at " + string.Join(':', connector.ConnectionData));
        }
        else
        {
            Log.WriteToFile("Successfully started at " + string.Join(':', connector.ConnectionData));
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public static async Task JoinGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        
        CurrentConnector = connector;
        await connector.Init();

        Log.WriteToFile($"==== JOINING TO: {string.Join(':', connector.ConnectionData)} ====");

        if (await connector.TryJoinGame() == false)
        {
            Log.WriteToFile("Error: Failed to join to " + string.Join(':', connector.ConnectionData));
        }
        
        NetworkManager.Singleton.NetworkConfig.NetworkTransport.OnTransportEvent += OnNetworkTransportEvent;
    }

    // For clients only.
    private static void OnNetworkTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
    {
        switch (eventType)
        {
            case NetworkEvent.Connect:
                Log.WriteToFile($"Successfully connected to {string.Join(':', CurrentConnector.ConnectionData)}");
                break;
            case NetworkEvent.Disconnect:
                Log.WriteToFile($"Error: Failed to connect to {string.Join(':', CurrentConnector.ConnectionData)}");
                break;
        }
        
        NetworkManager.Singleton.NetworkConfig.NetworkTransport.OnTransportEvent -= OnNetworkTransportEvent;
    }

    private static void OnClientConnected(ulong id)
    {
        Log.WriteToFile($"Client connected. ID: '{id}'");
    }

    private static void OnClientDisconnected(ulong id)
    {
        Log.WriteToFile($"Client disconnected. ID: '{id}'");
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
