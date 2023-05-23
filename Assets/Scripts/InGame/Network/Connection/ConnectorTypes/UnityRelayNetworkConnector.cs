using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;

public class UnityRelayNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData => new [] { _joinCode };
    
    private string _joinCode;

    public UnityRelayNetworkConnector(IReadOnlyList<string> connectionData)
    {
        _joinCode = connectionData[0];
    }

    public async void CreateGame() 
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }
        
        await TryAuthenticate();
        
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync((int)NetworkConnectorHandler.MaxPlayersAmount);
        _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
        
        NetworkManager.Singleton.Shutdown();

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Desk", LoadSceneMode.Single);
    }
    
    public async void JoinGame() 
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }
        
        await TryAuthenticate();
        
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);

        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        unityTransport.SetClientRelayData(allocation.RelayServer.IpV4, 
            (ushort)allocation.RelayServer.Port, 
            allocation.AllocationIdBytes, 
            allocation.Key, 
            allocation.ConnectionData, 
            allocation.HostConnectionData);
        
        NetworkManager.Singleton.Shutdown();
        
        NetworkManager.Singleton.StartClient();
    }    
    
    private static async Task TryAuthenticate() 
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Initialized || AuthenticationService.Instance.IsAuthorized == true)
            {
                return;
            }
        }
        catch (ServicesInitializationException) { }

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
}