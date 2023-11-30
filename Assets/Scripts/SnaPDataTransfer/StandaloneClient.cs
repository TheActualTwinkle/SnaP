using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace SnaPDataTransfer
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
        
        private async void Start() // TODO: This is has to be invoked via buttons or something like that. Return value has to be lobbies list.
        {
            TcpClient tcpClient;
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(_serverIpAddress, _serverPort);
            }
            catch (Exception e)
            {
                Logger.Log($"Can`t connect to {_serverIpAddress}:{_serverPort}. {e}", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                throw;
            }
            
            Logger.Log($"Connected to server at {_serverIpAddress}:{_serverPort}.", Logger.LogSource.SnaPDataTransfer);

            NetworkStream stream = tcpClient.GetStream();
            
            await GetLobbyInfoAsync(stream);
            
            tcpClient?.Close();
        }
        
        private void OnDestroy()
        {
            _destroyed = true;
        }
        
        private async Task<List<LobbyInfo>> GetLobbyInfoAsync(Stream stream)
        {
            string message = GetMessage();

            byte[] data = Encoding.ASCII.GetBytes(message);
                
            // Send "get" or "close" request.
            await stream.WriteAsync(data, 0, data.Length);
            Logger.Log($"Send {message}.", Logger.LogSource.SnaPDataTransfer);

            // If we are dead - return.
            // But before send "close" request to server.
            if (_destroyed == true)
            {
                return null;
            }
            
            // Getting count of lobbies.
            var buffer = new byte[BufferSize];
            int read = await stream.ReadAsync(buffer);
            string lengthString = Encoding.ASCII.GetString(buffer, 0, read);
            
            if (int.TryParse(lengthString, out int length) == false)
            {
                Logger.Log($"Can`t parse {lengthString} to int.", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                return null;
            }
            
            Logger.Log($"Received {length} lobbies count.", Logger.LogSource.SnaPDataTransfer);
            
            // Using the count of lobbies, get all lobbies info. 
            for (var i = 0; i < length; i++)
            {
                var response = new byte[BufferSize];
                string responseString = Encoding.ASCII.GetString(response); // TODO: Parse json to LobbyInfo and save it somewhere here to list.
                Logger.Log($"[STANDALONE] Received: {responseString}", Logger.LogSource.SnaPDataTransfer);
            }

            return null; // TODO: return lobbies info from jsons.
        }
        
        private string GetMessage()
        {
            if (_destroyed == true)
            {
                return "close";
            }

            return "get";
        }
    }
}