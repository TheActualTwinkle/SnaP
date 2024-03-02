using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour, ILoadingOperation
{
    public string Description => "Player spawning...";
    
    [SerializeField] private Player _playerPrefab;
    
    private ulong _clientId;
    private string _sceneName = string.Empty;

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
    }

    public Task Load(Action<float> onProgress)
    {
        onProgress?.Invoke(Constants.Loading.FakeLoadStartValue);
        
        Spawn();
        
        onProgress?.Invoke(1);
        
        return Task.CompletedTask;
    }

    private void Spawn()
    {
        if (_sceneName.Contains(Constants.SceneNames.Desk) == true && IsServer == true)
        {
            if (IsHost == false && NetworkManager.Singleton.LocalClientId == _clientId)
            {
                return;
            }
            
            NetworkObjectSpawner.SpawnNetworkObjectChangeOwnershipToClient(_playerPrefab.gameObject, Vector3.zero, _clientId, true).GetComponent<Player>();
        }
    }
    
    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        _clientId = clientId;
        _sceneName = sceneName;
    }
}