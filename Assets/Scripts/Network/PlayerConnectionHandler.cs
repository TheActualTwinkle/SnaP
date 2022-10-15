using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerConnectionHandler : NetworkBehaviour
{
    public static PlayerConnectionHandler Instance => _instance;
    private static PlayerConnectionHandler _instance;

    [SerializeField] private Player _playerPrefab;

    private List<Player> _connectedPlayers = new List<Player>();

    private void OnEnable()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
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
        }   
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    public Player GetConnectedPlayer(ulong clientId)
    {
        return _connectedPlayers.Where(x => x.NetworkObjectId == clientId).FirstOrDefault();
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName.Contains("Menu") == false && IsOwnedByServer == true)
        {
            Player player = NetworkObjectSpawner.SpawnNetworkObjectChangeOwnershipToClient(_playerPrefab.gameObject, Vector3.zero, clientId, true).GetComponent<Player>();
            Debug.Log($"'{player.NickName}' joins server");
            _connectedPlayers.Add(player);
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        Player player = GetConnectedPlayer(clientId);
         
        if (player != null)
        {
            _connectedPlayers.Remove(player);
        }

        if (clientId == 0)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}