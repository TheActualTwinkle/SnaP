using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class DedicatedServerNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData { get; private set; }
        
    private string _ipAddress;
    private string _port;

    public async Task Init()
    {
        string[] args = Environment.GetCommandLineArgs();
        if (args.Contains("-pubip") == true)
        {
            _ipAddress = await IpAddressProvider.GetPublic();
        }
        else
        {
            _ipAddress = await IpAddressProvider.GetLocal();
        }

        int portArgIndex = Array.IndexOf(args, "-port");
        _port = portArgIndex != -1 ? args[portArgIndex + 1] : "47924";

        ConnectionData = new[] {_ipAddress, _port};
    }

    public Task<bool> TryCreateGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return Task.FromResult(false);
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        if (ushort.TryParse(_port, out ushort port) == false)
        {
            return Task.FromResult(false);
        }
        unityTransport.SetConnectionData(_ipAddress, port);

        NetworkManager.Singleton.Shutdown();

        try
        {
            NetworkManager.Singleton.StartServer();
            NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.Desk, LoadSceneMode.Single);
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }

    public Task<bool> TryJoinGame()
    {        
        if (NetworkManager.Singleton.IsListening == true)
        {
            return Task.FromResult(false);
        }

        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        
        if (ushort.TryParse(_port, out ushort port) == false)
        {
            return Task.FromResult(false);
        }
        unityTransport.SetConnectionData(_ipAddress, port);
        
        NetworkManager.Singleton.Shutdown();

        try
        {
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }
}
