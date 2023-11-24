using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

/// <summary>
/// Use this for star connection on dedicated server.
/// </summary>
public class DedicatedServerNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData { get; private set; }
        
    private string _ipAddress;
    private string _port;

    public async Task Init()
    {
        _ipAddress = await IpAddressProvider.GetLocalAsync();

        string[] args = Environment.GetCommandLineArgs();
        int portArgIndex = Array.IndexOf(args, "-port");
        _port = portArgIndex != -1 ? args[portArgIndex + 1] : "47924";

        ConnectionData = new[] {_ipAddress, _port};
    }

    public async Task<bool> TryCreateGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            return false;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        if (ushort.TryParse(_port, out ushort port) == false)
        {
            return false;
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
            return false;
        }
 
        Logger.Log("Forwarding to public IP...");
        ConnectionData = new[] {await IpAddressProvider.GetPublicAsync(), _port};
        
        return true;
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
