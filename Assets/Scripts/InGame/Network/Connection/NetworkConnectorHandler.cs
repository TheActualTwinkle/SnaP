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
    private const uint ConnectTimeoutMS = 3000;
    
    [UsedImplicitly] private const uint DelayBeforeErrorShutdownMS = 3000;
    
    private const string ConnectionDataSeparator = ":";
    
    private static bool _isSubscribedToUserConnectionEvents;
    
    public static async Task CreateGame(NetworkConnectorType connectorType)
    {
        if (State == ConnectionState.Connecting)
        {
            Logger.Log("Already connecting. Aborting...", Logger.LogLevel.Error);
            return;
        }

        INetworkConnector connector = NetworkConnectorFactory.Get(connectorType);
        
        Connector = connector;
        await connector.Init();

#if !UNITY_EDITOR
        Logger.Log("=============================================================");
#endif
        Logger.Log($"Starting at: {string.Join(ConnectionDataSeparator, connector.ConnectionData)}");
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

            if (await connector.TryCreateGame() == false)
            {
                Logger.Log($"Failed to start at {string.Join(ConnectionDataSeparator, connector.ConnectionData)}. Attempt {i+1}/{MaxConnectAttempts}", Logger.LogLevel.Error);
                await Task.Delay((int)ConnectTimeoutMS);
                
                if (i == MaxConnectAttempts - 1)
                {
#if !UNITY_STANDALONE
                    Logger.Log($"Connection timeout. Shuts-downing in {DelayBeforeErrorShutdownMS} milliseconds.");
                    await Task.Delay((int)DelayBeforeErrorShutdownMS);
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
        Logger.Log("Successfully started at " + string.Join(ConnectionDataSeparator, connector.ConnectionData));
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

        INetworkConnector connector = NetworkConnectorFactory.Get(connectorType);
        
        Connector = connector;
        await connector.Init();

        Logger.Log($"==== JOINING TO: {string.Join(ConnectionDataSeparator, connector.ConnectionData)} ====");

        if (await connector.TryJoinGame() == false)
        {
            Logger.Log("Failed to join to " + string.Join(ConnectionDataSeparator, connector.ConnectionData), Logger.LogLevel.Error);
            State = ConnectionState.Failed;
            ConnectionStateChangedEvent?.Invoke(State);
            return;
        }
        
        NetworkManager.Singleton.NetworkConfig.NetworkTransport.OnTransportEvent += OnNetworkTransportEvent;
    }

    // For clients only.
    private static void OnNetworkTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
    {
        switch (eventType)
        {
            case NetworkEvent.Connect:
                Logger.Log($"Successfully connected to {string.Join(ConnectionDataSeparator, Connector.ConnectionData)}");
                State = ConnectionState.Successful;
                break;
            case NetworkEvent.Disconnect:
                Logger.Log($"Failed to connect to {string.Join(ConnectionDataSeparator, Connector.ConnectionData)}", Logger.LogLevel.Error);
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
