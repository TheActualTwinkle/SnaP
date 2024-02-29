using System;
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

    public Task Init()
    {
        IReadOnlyList<string> connectionData = ConnectionInputFields.Instance.GetConnectionData(NetworkConnectorType.UnityRelay);
        
        _joinCode = connectionData[0];
        
        return Task.CompletedTask;
    }

    public async Task<bool> TryCreateGame() 
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            Logger.Log("Can`t create game: NetworkManager is already listening.", Logger.LogLevel.Error);
            return false;
        }
        
        await Authenticate();
        
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync((int)NetworkConnectorHandler.MaxPlayersAmount);
        _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        unityTransport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
        
        NetworkManager.Singleton.Shutdown();

        try
        {
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.Desk, LoadSceneMode.Single);
        }
        catch (Exception e)
        {
            Logger.Log($"Can`t StartHost(). {e}", Logger.LogLevel.Error);
            return false;
        }
        
        return true;
    }
    
    public async Task<bool> TryJoinGame() 
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            Logger.Log("Can`t join game: NetworkManager is already listening.", Logger.LogLevel.Error);
            return false;
        }
        
        await Authenticate();

        JoinAllocation allocation;
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);
        }
        catch (Exception e)
        {
            Logger.Log($"Can`t join game: {e.Message}", Logger.LogLevel.Error);
            return false;
        }

        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        unityTransport.SetClientRelayData(allocation.RelayServer.IpV4, 
            (ushort)allocation.RelayServer.Port, 
            allocation.AllocationIdBytes, 
            allocation.Key, 
            allocation.ConnectionData, 
            allocation.HostConnectionData);
        
        NetworkManager.Singleton.Shutdown();

        try
        {
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Logger.Log($"Can`t StartClient(). {e}", Logger.LogLevel.Error);
            return false;
        }

        return true;
    }    
    
    private static async Task Authenticate() 
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            //If not already logged, log the user in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}