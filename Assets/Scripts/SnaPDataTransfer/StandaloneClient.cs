using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace SDT
{
    /// <summary>
    /// <para> This should be used ONLY in a dedicated standalone builds. </para> 
    /// <para> Read data from the SnaPDataTransfer server. </para>
    /// </summary>
    public class StandaloneClient : MonoBehaviour
    {
        private static StandaloneClient Instance { get; set; }

        private const uint BufferSize = 512;
        
        [SerializeField] private string _serverIpAddress; // TODO: Make it real ip address.
        [SerializeField] private int _serverPort;

        private bool _destroyed;

        private NetworkStream _networkStream;
        private TcpClient _tcpClient;
        
        private void Awake()
        {
#if !PLATFORM_STANDALONE
            Logger.Log("StandaloneClient should be used ONLY in a standalone builds!", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
            Destroy(gameObject);
            return;
#endif
            
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private async void Start()
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_serverIpAddress, _serverPort);
            }
            catch (Exception e)
            {
                Logger.Log($"Can`t connect to {_serverIpAddress}:{_serverPort}. {e}", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                throw;
            }
            
            Logger.Log($"Connected to server at {_serverIpAddress}:{_serverPort}.", Logger.LogSource.SnaPDataTransfer);

            _networkStream = _tcpClient.GetStream();
        }
        
        private void OnDestroy()
        {
            _tcpClient?.Close();
            _destroyed = true;
        }

        public async Task<List<LobbyInfo>> GetLobbyInfoAsync()
        {
            try
            {
                return await GetLobbyInfoAsyncInternal();
            }
            catch (Exception e)
            {
                Logger.Log($"Can`t get lobby info. {e}", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                return new List<LobbyInfo>();                
            }
        }

        
        private async Task<List<LobbyInfo>> GetLobbyInfoAsyncInternal()
        {
            string message = _destroyed ? "close" : "get-count";
            
            // Send "get-count" or "close" request.
            await WriteAsync(message);

            // If we are dead - return.
            // But before send "close" request to server.
            if (_destroyed == true)
            {
                return new List<LobbyInfo>();
            }
            
            // Getting count of lobbies.
            string lengthString = await ReadAsync();
            
            if (int.TryParse(lengthString, out int length) == false)
            {
                Logger.Log($"Can`t parse {lengthString} to int.", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                return null;
            }
            
            Logger.Log($"Received {length} lobbies count.", Logger.LogSource.SnaPDataTransfer);
            
            message = "get-info";
            await WriteAsync(message);

            List<LobbyInfo> lobbyInfos = new();
            // Using the count of lobbies, get all lobbies info. 
            for (var i = 0; i < length; i++)
            {
                string response = await ReadAsync();
                Logger.Log($"[STANDALONE] Received: {response}", Logger.LogSource.SnaPDataTransfer);

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
        
        private async Task<string> ReadAsync()
        {
            var buffer = new byte[BufferSize];
            int read = await _networkStream.ReadAsync(buffer);
            string message = Encoding.ASCII.GetString(buffer, 0, read);

            return message;
        }
        
        private async Task WriteAsync(string message)
        { 
            byte[] data = Encoding.ASCII.GetBytes(message);
                
            await _networkStream.WriteAsync(data, 0, data.Length);
            Logger.Log($"Send {message}.", Logger.LogSource.SnaPDataTransfer);
        }
    }
}