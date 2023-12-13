using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace SDT
{
    /// <summary>
    /// <para> This must be used ONLY if you are SnaP SERVER. </para> 
    /// <para> Send data to the SnaPDataTransfer server. </para>
    /// </summary>
    public class Server : MonoBehaviour
    {
        private static Server Instance { get; set; }

        [SerializeField] private string _serverIpAddress;
        [SerializeField] private int _serverPort;

        [SerializeField] private int _awaitLobbyInitializationIntervalMs;

        private bool _destroyed;
        
        private TcpClient _tcpClient;
        private NetworkStream Stream => _tcpClient.GetStream();

        private void Awake()
        {
            // If this is a client, then destroy this object.
            if (NetworkManager.Singleton.IsServer == false)
            {
                Destroy(gameObject);
                return;
            }
            
            UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

            // We cant use SDT with unity relay.
            if (unityTransport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
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

            await SendDataUntilInitialized();
            
            PlayerSeats.Instance.PlayerSitEvent += (_, _) =>
            {
                SendData();
            };            
            
            PlayerSeats.Instance.PlayerLeaveEvent += (_, _) =>
            {
                SendData();
            };
        }

        private void OnDestroy()
        {
            _destroyed = true;
            _tcpClient?.Close();
        }

        private async Task SendDataUntilInitialized()
        {
            while (true)
            {
                SendData();

                if (IsLobbyInitialized() == true)
                {
                    break;
                }
                
                await Task.Delay(_awaitLobbyInitializationIntervalMs);
            }
        }
        
        private async void SendData()
        {
            string message = await GetMessage();

            byte[] data = Encoding.ASCII.GetBytes(message);
            await Stream.WriteAsync(data, 0, data.Length);
        }

        private async Task<string> GetMessage()
        {
            if (_destroyed == true)
            {
                return "close";
            }

            if (IsLobbyInitialized() == false)
            {
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