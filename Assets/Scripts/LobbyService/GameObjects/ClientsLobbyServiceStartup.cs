using LobbyService.Interfaces;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class ClientsLobbyServiceStartup : MonoBehaviour
{
    private IClientsLobbyService _clientsLobbyService;
    
    [Inject]
    private void Construct(IClientsLobbyService clientsLobbyService)
    {
        _clientsLobbyService = clientsLobbyService;
    }
    
    private void Awake()
    {
        // If this is a client, then destroy this object.
        if (Application.platform is RuntimePlatform.WindowsServer or RuntimePlatform.LinuxServer)
        {
            // Logger.Log("StandaloneClient should be used ONLY in a standalone builds!", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
            Destroy(gameObject);
            return;
        }
            
        // If this is a server, then destroy this object.
        if (NetworkManager.Singleton.IsServer == true)
        {
            // Logger.Log("ClientsLobbyServiceStartup should be used ONLY as a SnaP client!", Logger.LogLevel.Error, Logger.LogSource.LobbyService);
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        if (_clientsLobbyService == null) return;
        
        await _clientsLobbyService.Start();
    }

    private async void OnDestroy()
    {
        if (_clientsLobbyService == null) return;

        await _clientsLobbyService.Stop();
    }
}
