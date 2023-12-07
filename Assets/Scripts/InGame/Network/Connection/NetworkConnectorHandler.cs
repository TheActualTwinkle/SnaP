using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public static class NetworkConnectorHandler
{
    public const uint MaxPlayersAmount = 5;

    private const uint MaxConnectAttempts = 4;
    private const uint ConnectTimeoutMS = 3000;
    
    private const uint DelayBeforeErrorShutdownMS = 3000;
    
    public static INetworkConnector CurrentConnector { get; private set; }

    private static bool _isSubscribedToUserConnectionEvents; 
    
    public static async Task CreateGame(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = GetConnector(connectorType);
        
        CurrentConnector = connector;
        await connector.Init();

#if !UNITY_EDITOR
        Logger.Log("=============================================================");
#endif
        Logger.Log($"Starting at: {string.Join(':', connector.ConnectionData)}");
#if !UNITY_EDITOR
        Logger.Log("=============================================================");
#endif

        for (var i = 0; i < MaxConnectAttempts; i++)
        {
            if (await connector.TryCreateGame() == false)
            {
                Logger.Log($"Failed to start at {string.Join(':', connector.ConnectionData)}. Attempt {i+1}/{MaxConnectAttempts}", Logger.LogLevel.Error);
                await Task.Delay((int)ConnectTimeoutMS);
                
                if (i == MaxConnectAttempts - 1)
                {
#if !UNITY_STANDALONE
                    Logger.Log($"Connection timeout. Shuts-downing in {DelayBeforeErrorShutdownMS} milliseconds.");
                    await Task.Delay((int)DelayBeforeErrorShutdownMS);
                    Application.Quit(-1);
#endif
                    return;
                }
                
                continue;
            }
            
            break;
        }
        
#if !UNITY_EDITOR
        Logger.Log("=============================================================");
#endif
        Logger.Log("Successfully started at " + string.Join(':', connector.ConnectionData));
#if !UNITY_EDITOR
        Logger.Log("=============================================================");
#endif


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
            Logger.Log("Failed to join to " + string.Join(':', connector.ConnectionData), Logger.LogLevel.Error);
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
                Logger.Log($"Failed to connect to {string.Join(':', CurrentConnector.ConnectionData)}", Logger.LogLevel.Error);
                break;
        }
        
        NetworkManager.Singleton.NetworkConfig.NetworkTransport.OnTransportEvent -= OnNetworkTransportEvent;
    }

    // For server only.
    private static void OnClientConnected(ulong id)
    {
        Logger.Log($"User connected. ID: '{id}'");
    }

    // For server only.
    private static void OnClientDisconnected(ulong id)
    {
        Logger.Log($"User disconnected. ID: '{id}'");
    }
    
    private static INetworkConnector GetConnector(NetworkConnectorType connectorType)
    {
        INetworkConnector connector = connectorType switch
        {
            NetworkConnectorType.IpAddress => new IPAddressNetworkConnector(),
            NetworkConnectorType.UnityRelay => new UnityRelayNetworkConnector(),
            NetworkConnectorType.DedicatedServer => new DedicatedServerNetworkConnector(),
            NetworkConnectorType.UPnP => new UPnPNetworkConnector(),
            _ => throw new ArgumentOutOfRangeException(nameof(connectorType), connectorType, null)
        };
        
        return connector;
    }
}
