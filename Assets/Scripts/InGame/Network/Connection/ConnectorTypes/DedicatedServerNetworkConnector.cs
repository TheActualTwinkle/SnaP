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
        .ToString(), "4792"};
        
    public void Init()
    {
        return;
    }

    public void CreateGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        var ipAddress = Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
        
        unityTransport.SetConnectionData(ipAddress, 4792, "0.0.0.0"); // todo real server IP
        Debug.Log("Starting at: " + string.Join(':', ConnectionData));

        NetworkManager.Singleton.Shutdown();

        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.SceneManager.LoadScene("Desk_d", LoadSceneMode.Single);
    }

    public void JoinGame()
    {        
        if (NetworkManager.Singleton.IsListening == true)
        {
            return;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        unityTransport.SetConnectionData("192.168.0.14", 4792); // todo
        
        NetworkManager.Singleton.Shutdown();
        
        NetworkManager.Singleton.StartClient();
    }
}
