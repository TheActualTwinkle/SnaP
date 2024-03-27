using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

public static class NetworkConnectorHandler
{
    public enum ConnectionState : byte
    {
        Disconnected,
        Canceled,
        Connecting,
        Successful,
        Failed,
    }
    
    public static event Action<ConnectionState> ConnectionStateChangedEvent;
    
    public static INetworkConnector Connector { get; private set; }

    public static ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    public static bool ShutdownTrigger;

    public const uint MaxPlayersAmount = 5;
    

    private const uint MaxConnectAttempts = 4;
    private const uint ConnectTimeoutMs = 3000;
    
    [UsedImplicitly] private const uint DelayBeforeErrorShutdownMS = 3000;
    
    private static bool _isSubscribedToUserConnectionEvents;
    
    public static async Task CreateGame(NetworkConnectorType connectorType)
    {
        if (State == ConnectionState.Connecting)
        {
            Logger.Log("Already connecting. Aborting...", Logger.LogLevel.Error);
            return;
        }

        Connector = NetworkConnectorFactory.Get(connectorType);
        await Connector.Init();

#if !UNITY_EDITOR
        Logger.Log("=============================================================");
#endif
        Logger.Log($"Starting via {connectorType}...");
#if !UNITY_EDITOR
        Logger.Log("=============================================================");
#endif

        for (var i = 0; i < MaxConnectAttempts; i++)
        {
            if (ShutdownTrigger == true)
            {
                ShutdownTrigger = false;
                State = ConnectionState.Canceled;
                ConnectionStateChangedEvent?.Invoke(State);
                return;
            }

            if (await Connector.TryCreateGame() == false)
            {
                Logger.Log($"Failed to start at {await ConnectionDataFactory.Get(connectorType)}. Attempt {i+1}/{MaxConnectAttempts}", Logger.LogLevel.Error);
                await Task.Delay(TimeSpan.FromMilliseconds(ConnectTimeoutMs));
                
                if (i == MaxConnectAttempts - 1)
                {
#if !UNITY_STANDALONE
                    Logger.Log($"Connection timeout. Shuts-downing in {DelayBeforeErrorShutdownMS} milliseconds.");
                    await Task.Delay(TimeSpan.FromMilliseconds(DelayBeforeErrorShutdownMS));
                    Application.Quit(-1);
#endif
                    State = ConnectionState.Canceled;
                    ConnectionStateChangedEvent?.Invoke(State);
                    return;
                }
                
                continue;
            }
            
            break;
        }
        
#if !UNITY_EDITOR
        Logger.Log("=============================================================");
#endif
        Logger.Log($"Successfully started at {await ConnectionDataFactory.Get(connectorType)}");
#if !UNITY_EDITOR
        Logger.Log("=============================================================");
#endif


        if (_isSubscribedToUserConnectionEvents == false)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            _isSubscribedToUserConnectionEvents = true;
        }

        State = ConnectionState.Successful;
        ConnectionStateChangedEvent?.Invoke(State);
    }

    public static async Task JoinGame(NetworkConnectorType connectorType)
    {
        if (State == ConnectionState.Connecting)
        {
            Logger.Log($"Already connecting. Aborting...", Logger.LogLevel.Error);
            return;
        }

        Connector = NetworkConnectorFactory.Get(connectorType);
        await Connector.Init();

        Logger.Log($"Joining via {connectorType}");

        if (await Connector.TryJoinGame() == false)
        {
            Logger.Log($"Failed to join to {await ConnectionDataFactory.Get(connectorType)}", Logger.LogLevel.Error);
            State = ConnectionState.Failed;
            ConnectionStateChangedEvent?.Invoke(State);
            return;
        }
        
        NetworkManager.Singleton.NetworkConfig.NetworkTransport.OnTransportEvent += OnNetworkTransportEvent;
    }

    // For clients only.
    private static async void OnNetworkTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
    {
        switch (eventType)
        {
            case NetworkEvent.Connect:
                Logger.Log($"Successfully connected to {await ConnectionDataFactory.Get(Connector.Type)}");
                State = ConnectionState.Successful;
                break;
            case NetworkEvent.Disconnect:
            case NetworkEvent.TransportFailure: 
                Logger.Log($"Failed to connect to {await ConnectionDataFactory.Get(Connector.Type)}", Logger.LogLevel.Error);
                State = ConnectionState.Failed;
                break;
        }
        
        ConnectionStateChangedEvent?.Invoke(State);
        
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
}
