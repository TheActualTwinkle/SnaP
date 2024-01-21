﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Netcode;

namespace SDT
{
    /// <summary>
    /// <para> You must be a CLIENT and use it ONLY in a standalone builds. </para> 
    /// <para> Read data from the SnaPDataTransfer server. </para>
    /// </summary>
    public class Client : MonoBehaviour
    {
        public event Action<ConnectionState> ConnectionStateChangedEvent;
        
        public static Client Instance { get; private set; }

        public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;

        private const uint BufferSize = 512;
        
        private const string GetCountCommand = "get-count";
        private const string GetInfoCommand = "get-info";
        private const string CloseCommand = "close";
        
        [SerializeField] private string _serverIpAddress;
        [SerializeField] private ushort _serverPort;

        private bool _destroyed;

        private TcpClient _tcpClient;
        private NetworkStream NetworkStream => _tcpClient.GetStream();

        private void Awake()
        {
            if (Application.platform is RuntimePlatform.WindowsServer or RuntimePlatform.LinuxServer)
            {
                Logger.Log("StandaloneClient should be used ONLY in a standalone builds!", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                Destroy(gameObject);
                return;
            }
            
            // If this is a server, then destroy this object.
            if (NetworkManager.Singleton.IsServer == true)
            {
                Destroy(gameObject);
                return;
            }
            
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            Connect();
        }
        
        private void OnDestroy()
        {
            ConnectionState = ConnectionState.Abandoned;
            ConnectionStateChangedEvent?.Invoke(ConnectionState.Abandoned);
            
            _tcpClient?.Close();
            _destroyed = true;
            
            Instance = null;
        }

        public void Reconnect()
        {
            Disconnect();
            Connect();
        }
        
        private async void Connect()
        {
            ConnectionState = ConnectionState.Connecting;
            ConnectionStateChangedEvent?.Invoke(ConnectionState.Connecting);
            
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_serverIpAddress, _serverPort);
            }
            catch (Exception e) // todo CHECK IF THE SERVER ERROR IS SAME WITH CLIENT ERROR.
            {
                if (ConnectionState == ConnectionState.Abandoned)
                {
                    return;
                }
                
                ConnectionState = ConnectionState.Failed;
                ConnectionStateChangedEvent?.Invoke(ConnectionState.Failed);
                
                Logger.Log($"Can`t connect to {_serverIpAddress}:{_serverPort}. {e}", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                return;
            }
            
            Logger.Log($"Connected to server at {_serverIpAddress}:{_serverPort}.", Logger.LogSource.SnaPDataTransfer);

            ConnectionState = ConnectionState.Successful;
            ConnectionStateChangedEvent?.Invoke(ConnectionState.Successful);
        }
        
        private async void Disconnect()
        {
            ConnectionState = ConnectionState.Disconnected;
            ConnectionStateChangedEvent?.Invoke(ConnectionState.Disconnected);

            
            if (_tcpClient is { Connected: true })
            {
                await WriteAsync(CloseCommand);
            }

            _tcpClient?.Close();
        }
        
        public async Task<List<LobbyInfo>> GetLobbiesInfoAsync(CancellationTokenSource token)
        {
            try
            {
                return await GetLobbyInfoAsyncInternal(token);
            }
            catch (Exception e) // todo CHECK IF THE SERVER ERROR IS SAME WITH CLIENT ERROR; to make different errors.
            {
                if (ConnectionState == ConnectionState.Abandoned)
                {
                    return null;
                }
                
                ConnectionState = ConnectionState.Failed;
                ConnectionStateChangedEvent?.Invoke(ConnectionState.Failed);
                
                Logger.Log($"Can`t get lobby info. {e}", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                return null;                
            }
        }

        private async Task<List<LobbyInfo>> GetLobbyInfoAsyncInternal(CancellationTokenSource token)
        {
            string message = _destroyed ? CloseCommand : GetCountCommand;
            
            // Send "get-count" or "close" request.
            await WriteAsync(message);

            // If we are dead - return.
            // But before send "close" request to server.
            if (_destroyed == true)
            {
                return new List<LobbyInfo>();
            }
            
            int length = await GetCountOfLobbies();

            Logger.Log($"Received {length} lobbies count.", Logger.LogSource.SnaPDataTransfer);

            List<LobbyInfo> lobbyInfos = new();
            // Using the count of lobbies, get all lobbies info. 
            for (var i = 0; i < length; i++)
            {
                if (token.IsCancellationRequested == true)
                {
                    return lobbyInfos;
                }

                message = $"{GetInfoCommand} {i}";
                await WriteAsync(message);
                
                string response = await ReadAsync();
                Logger.Log($"Received: {response}", Logger.LogSource.SnaPDataTransfer);

                // Parsing json to LobbyInfo[] and return it.
                try
                {
                    LobbyInfo lobbyInfo = JsonConvert.DeserializeObject<LobbyInfo>(response);
                    lobbyInfos.Add(lobbyInfo);
                }
                catch (Exception e)
                {
                    Logger.Log($"Can`t deserialize json to LobbyInfo at {i}/{length-1}. " + e, Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                }
            }

            return lobbyInfos;
        }

        private async Task<int> GetCountOfLobbies()
        {
            // Getting count of lobbies.
            string lengthString = await ReadAsync();

            if (int.TryParse(lengthString, out int length) == false)
            {
                Logger.Log($"Can`t parse {lengthString} to int.", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                return -1;
            }

            return length;
        }

        private async Task<string> ReadAsync()
        {
            var buffer = new byte[BufferSize];

            int read = await NetworkStream.ReadAsync(buffer);
            string message = Encoding.ASCII.GetString(buffer, 0, read);

            return message;
        }
        
        private async Task WriteAsync(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            
            await NetworkStream.WriteAsync(data, 0, data.Length);
            Logger.Log($"Send {message}.", Logger.LogSource.SnaPDataTransfer);
        }
    }
}