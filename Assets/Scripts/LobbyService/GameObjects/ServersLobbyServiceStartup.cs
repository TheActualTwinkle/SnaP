using LobbyService.Interfaces;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Zenject;

public class ServersLobbyServiceStartup : MonoBehaviour
{
    private IServersLobbyService _serversLobbyService;
    
    [Inject]
    private void Construct(IServersLobbyService serversLobbyService)
    {
        _serversLobbyService = serversLobbyService;
    }
    
    private void Awake()
    {
        // If this is a client, then destroy this object.
        if (NetworkManager.Singleton.IsServer == false)
        {
            // Logger.Log("ServersLobbyServiceStartup should be used ONLY as a SnaP server!", Logger.LogLevel.Warning, Logger.LogSource.LobbyService);
            Destroy(gameObject);
            return;
        }
            
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        // We cant use SDT with unity relay.
        if (unityTransport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
        {
            // Logger.Log("Unity relay transport is not supported by SDT!", Logger.LogLevel.Warning, Logger.LogSource.LobbyService);
            Destroy(gameObject);
        }
    }
        
    private async void Start()
    {
        if (_serversLobbyService == null) return;
        
        await _serversLobbyService.Start();
    }

    private async void OnDestroy()
    {
        if (_serversLobbyService == null) return;
        
        await _serversLobbyService.Stop();
    }
}
