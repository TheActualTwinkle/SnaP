using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalAddressNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData => new [] { _ipAddress, _port };
    
    private readonly string _ipAddress;
    private readonly string _port;

    private IEnumerator _connectRoutine;

    public LocalAddressNetworkConnector(IReadOnlyList<string> connectionData)
    {
        _ipAddress = connectionData[0];
        _port = connectionData[1];
    }
    
    public void CreateGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        if (ushort.TryParse(_port, out ushort port) == false)
        {
            return;
        }
        unityTransport.SetConnectionData(_ipAddress, port);

        NetworkManager.Singleton.Shutdown();

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Desk", LoadSceneMode.Single);
    }

    public void JoinGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        unityTransport.SetConnectionData(_ipAddress, ushort.Parse(_port));
        
        NetworkManager.Singleton.Shutdown();
        
        NetworkManager.Singleton.StartClient();
    }
    
    // Button.
    private void Exit()
    {
        Application.Quit();
    }
}
