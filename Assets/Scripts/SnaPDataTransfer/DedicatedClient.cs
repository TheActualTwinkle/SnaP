using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace SDT
{
    /// <summary>
    /// <para> This should be used ONLY in a dedicated server builds. </para> 
    /// <para> Send data to the SnaPDataTransfer server. </para>
    /// </summary>
    public class DedicatedClient : MonoBehaviour
    {
        private static DedicatedClient Instance { get; set; }

        [SerializeField] private string _serverIpAddress;
        [SerializeField] private int _serverPort;

        [SerializeField] private int _awaitLobbyInitializationIntervalMs;
        [SerializeField] private int _sendDataIntervalMs;

        private int _interval;
        private bool _destroyed;
        
        private void Awake()
        {
#if PLATFORM_STANDALONE
#if !UNITY_EDITOR
            Logger.Log("DedicatedClient should be used ONLY in a dedicated server builds!", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
            Destroy(gameObject);
            return;   
#endif
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
            
            await EndlessHandleServerRequest(stream);
            
            tcpClient?.Close();
        }

        private void OnDestroy()
        {
            _destroyed = true;
        }

        private async Task EndlessHandleServerRequest(Stream stream)
        {
            while (true)
            {
                int interval = _sendDataIntervalMs;
                string message = await GetMessage();

                byte[] data = Encoding.ASCII.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                
                if (_destroyed == true)
                {
                    break;
                }
                
                await Task.Delay(_interval);
            }
        }

        private async Task<string> GetMessage()
        {
            if (_destroyed == true)
            {
                return "close";
            }

            if (IsLobbyInitialized() == false)
            {
                _interval = _awaitLobbyInitializationIntervalMs;
                return JsonConvert.SerializeObject(new LobbyInfo(string.Empty, 0, 0, 0, "Awaiting lobby initialization..."));
            }

            string ipAddress = await ConnectionDataPresenter.GetPublicIpAddressAsync();

            if (ushort.TryParse(NetworkConnectorHandler.CurrentConnector.ConnectionData.Last(), out ushort port) == false)
            {
                throw new ArgumentException($"Can`t parse port from {NetworkConnectorHandler.CurrentConnector.ConnectionData.Last()}");
            }

            return JsonConvert.SerializeObject(new LobbyInfo(
                ipAddress,
                port,
                PlayerSeats.MaxSeats,
                PlayerSeats.Instance.PlayersAmount + PlayerSeats.Instance.WaitingPlayersAmount,
                "TODO: Create a lobby name feature!" // TODO: Create a lobby name feature!
            ));
        }
        
        private static bool IsLobbyInitialized()
        {
            return PlayerSeats.Instance != null;
        }
    }   
}