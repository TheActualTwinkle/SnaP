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
        private enum DisconnectReason : byte
        {
            Unknown,
            PortClosed,
        }
        
        public event Action<ConnectionState> ConnectionStateChangedEvent;

        public static Server Instance { get; private set; }
        
        public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;

        private const string CloseCommand = "close";
        
        public string ServerIpAddress => _serverIpAddress;
        [SerializeField] private string _serverIpAddress;

        public ushort ServerPort => _serverPort;
        [SerializeField] private ushort _serverPort;

        [SerializeField] private int _awaitLobbyInitializationIntervalMs;

        private bool _destroyed;
        
        private TcpClient _tcpClient;
        private NetworkStream NetworkStream => _tcpClient.GetStream();

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

        private void Start()
        {
            Connect();
        }

        private void OnDestroy()
        {
            ConnectionState = ConnectionState.Abandoned;
            ConnectionStateChangedEvent?.Invoke(ConnectionState.Abandoned);
            
            _destroyed = true;
            _tcpClient?.Close();

            Instance = null;
        }

        public void Reconnect()
        {
            Disconnect(DisconnectReason.Unknown);
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
            catch (Exception e)
            {
                if (ConnectionState == ConnectionState.Abandoned)
                {
                    return;
                }
                
                ConnectionState = ConnectionState.Failed;
                ConnectionStateChangedEvent?.Invoke(ConnectionState.Failed);
                
                Logger.Log($"Can`t connect to {_serverIpAddress}:{_serverPort}. {e}", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                throw;
            }
            
            ConnectionState = ConnectionState.Successful;
            ConnectionStateChangedEvent?.Invoke(ConnectionState.Successful);
            
            Logger.Log($"Connected to server at {_serverIpAddress}:{_serverPort}.", Logger.LogSource.SnaPDataTransfer);

            try
            {
                await WaitUntilLobbyInitialized();
                
                ushort port = ConnectionDataPresenter.GetGamePort();
                
                await SendData(port);

                PlayerSeats.Instance.PlayerSitEvent += async (_, _) => { await SendData(port); };            
            
                PlayerSeats.Instance.PlayerLeaveEvent += async (_, _) => { await SendData(port); };            
            }
            catch (Exception e)
            {
                if (ConnectionState == ConnectionState.Abandoned)
                {
                    return;
                }
                
                ConnectionState = ConnectionState.Failed;
                ConnectionStateChangedEvent?.Invoke(ConnectionState.Failed);
                
                Logger.Log($"Exception while sending data to server. {e}", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
            }
        }

        // Sending some message based on Game Context/Connection situation.
        private async Task SendData(ushort port)
        {
            if (await UdpPortChecker.IsForwarded(port) == false)
            {
                Disconnect(DisconnectReason.PortClosed);
                return;
            }

            await WriteAsync();
        }

        private async void Disconnect(DisconnectReason reason)
        {
            ConnectionState newConnectionState = reason switch
            {
                DisconnectReason.Unknown => ConnectionState.Disconnected,
                DisconnectReason.PortClosed => ConnectionState.DisconnectedPortClosed,
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
            };

            ConnectionState = newConnectionState;
            ConnectionStateChangedEvent?.Invoke(ConnectionState);
         
            if (_tcpClient is { Connected: true })
            {
                await WriteAsync();
            }

            _tcpClient?.Close();
        }
        
        private async Task WaitUntilLobbyInitialized()
        {
            while (true)
            {
                if (IsLobbyInitialized() == true)
                {
                    break;
                }
                
                await Task.Delay(_awaitLobbyInitializationIntervalMs);
            }
        }
        
        private async Task WriteAsync()
        {
            string message = await GetMessage();
            
            byte[] data = Encoding.ASCII.GetBytes(message);
            await NetworkStream.WriteAsync(data, 0, data.Length);
        }

        private async Task<string> GetMessage()
        {
            if (_destroyed == true)
            {
                return CloseCommand;
            }
            
            if (ConnectionState is ConnectionState.DisconnectedPortClosed or ConnectionState.Disconnected)
            {
                return CloseCommand;
            }

            if (IsLobbyInitialized() == false)
            {
                return JsonConvert.SerializeObject(new LobbyInfo(string.Empty, 0, 0, 0, "Awaiting lobby initialization..."));
            }
            
            string ipAddress = await ConnectionDataPresenter.GetPublicIpAddressAsync();
            ushort port = ConnectionDataPresenter.GetGamePort();

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