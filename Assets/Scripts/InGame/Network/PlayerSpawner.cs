using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    public string Description => "Player spawning...";
    
    [SerializeField] private Player _playerPrefab;

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
    }
    
    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName.Contains(Constants.SceneNames.Desk) == true && IsServer == true)
        {
            if (IsHost == false && NetworkManager.Singleton.LocalClientId == clientId)
            {
                return;
            }
            
            NetworkObjectSpawner.SpawnNetworkObjectChangeOwnershipToClient(_playerPrefab.gameObject, Vector3.zero, clientId, true).GetComponent<Player>();
        }
    }
}