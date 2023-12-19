using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

// [Obsolete("Use UPnP connector instead.", false)]
public class IPAddressNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData => new [] { _ipAddress, _port };
    
    private string _ipAddress;
    private string _port;

    private IEnumerator _connectRoutine;
    
    public Task Init()
    {
        IReadOnlyList<string> connectionData = ConnectionInputField.Instance.GetConnectionData(NetworkConnectorType.IpAddress);

        _ipAddress = connectionData[0];
        _port = connectionData[1];
        
        return Task.CompletedTask;
    }
    
    public Task<bool> TryCreateGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            Logger.Log("Can`t create game: NetworkManager is already listening.", Logger.LogLevel.Error);
            return Task.FromResult(false);
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        if (ushort.TryParse(_port, out ushort port) == false)
        {
            Logger.Log($"Can`t parse port: {_port}", Logger.LogLevel.Error);
            return Task.FromResult(false);
        }
        unityTransport.SetConnectionData(_ipAddress, port);

        NetworkManager.Singleton.Shutdown();

        try
        {
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.Desk, LoadSceneMode.Single);
        }
        catch (Exception e)
        {
            Logger.Log($"Can`t StartHost(). {e}", Logger.LogLevel.Error);
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }

    public Task<bool> TryJoinGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            Logger.Log("Can`t join game: NetworkManager is already listening.", Logger.LogLevel.Error);
            return Task.FromResult(false);
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        
        if (ushort.TryParse(_port, out ushort port) == false)
        {
            Logger.Log($"Can`t parse port: {_port}", Logger.LogLevel.Error);
            return Task.FromResult(false);
        }
        unityTransport.SetConnectionData(_ipAddress, port);
        
        NetworkManager.Singleton.Shutdown();

        try
        {
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Logger.Log($"Can`t StartHost(). {e}", Logger.LogLevel.Error);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
    
    // Button.
    private void Exit()
    {
        Application.Quit();
    }
}
