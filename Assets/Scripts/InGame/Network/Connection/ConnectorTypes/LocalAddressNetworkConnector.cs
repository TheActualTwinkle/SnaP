using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalAddressNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData => new [] { _ipAddress, _port };
    
    private string _ipAddress;
    private string _port;

    private IEnumerator _connectRoutine;
    
    public void Init()
    {
        IReadOnlyList<string> connectionData = ConnectionInputField.Instance.GetConnectionData(NetworkConnectorType.LocalAddress);

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
        NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.Desk, LoadSceneMode.Single);
    }

    public void JoinGame()
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
        
        NetworkManager.Singleton.StartClient();
    }
    
    // Button. todo: Maybe it`s useless?
    private void Exit()
    {
        Application.Quit();
    }
}
