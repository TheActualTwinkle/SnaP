using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionHandler : NetworkBehaviour
{
    public static ConnectionHandler Instance { get; private set; }

    public static string ConnectionFullAdress => GetConnectionFullAdress();
    public static string LocalArdess => GetLocalAddress();

    [SerializeField] private Player _playerPrefab;

    private List<Player> _connectedPlayers = new List<Player>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }
    }

    public Player GetConnectedPlayer(ulong clientId)
    {
        return _connectedPlayers.Where(x => x.OwnerClientId == clientId).FirstOrDefault();
    }

    private static string GetLocalAddress()
    {
        IPHostEntry host;
        string localIP = "0.0.0.0";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }

    private static string GetConnectionFullAdress()
    {
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        string fullAdress = unityTransport.ConnectionData.Address + ":" + unityTransport.ConnectionData.Port;

        return fullAdress;
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName.Contains("Desk") == true && IsOwnedByServer == true)
        {
            Player player = NetworkObjectSpawner.SpawnNetworkObjectChangeOwnershipToClient(_playerPrefab.gameObject, Vector3.zero, clientId, true).GetComponent<Player>();
            _connectedPlayers.Add(player);
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        Player player = GetConnectedPlayer(clientId);        

        if (player != null)
        {
            Log.WriteLine($"Player ('{player.NickName}') is disconnected.");
            _connectedPlayers.Remove(player);
        }

        if (clientId == 0)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}