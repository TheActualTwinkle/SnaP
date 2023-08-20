using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DedicatedServerNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData => new [] {Dns.GetHostEntry(Dns.GetHostName())
        .AddressList.First(
            f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .ToString(), _port};
        
    private string _ipAddress;
    private string _port;

    public void Init()
    {
        _ipAddress = Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
        _port = "4792";
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
        unityTransport.SetConnectionData(_ipAddress, port, "0.0.0.0");
        Debug.Log("Starting at: " + string.Join(':', ConnectionData));

        NetworkManager.Singleton.Shutdown();

        NetworkManager.Singleton.StartServer();
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
}
