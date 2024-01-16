using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

/// <summary>
/// Use this for star game on dedicated server.
/// </summary>
public class DedicatedServerNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData { get; private set; }

    private string _ipAddress;
    private string _port;

    private const ushort DefaultPort = 47924;
    
    public async Task Init()
    {
        _ipAddress = (await ConnectionDataPresenter.GetLocalIpAddressAsync()).ToString();

        string[] args = Environment.GetCommandLineArgs();
        int portArgIndex = Array.IndexOf(args, "-port");
        _port = portArgIndex != -1 ? args[portArgIndex + 1] : DefaultPort.ToString();

        ConnectionData = new[] {_ipAddress, _port};
    }

    public async Task<bool> TryCreateGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            Logger.Log("Can`t create game: NetworkManager is already listening.", Logger.LogLevel.Error);
            return false;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        if (ushort.TryParse(_port, out ushort port) == false)
        {
            Logger.Log($"Can`t parse port: {_port}.", Logger.LogLevel.Error);
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
            Logger.Log($"Can`t StartServer().", Logger.LogLevel.Error);
            return false;
        }
 
        Logger.Log("Forwarding to public IP...");
        ConnectionData = new[] {await ConnectionDataPresenter.GetPublicIpAddressAsync(), _port};
        
        return true;
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
            Logger.Log($"Can`t parse port: {_port}.", Logger.LogLevel.Error);
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
            Logger.Log($"Can`t StartClient(). {e}", Logger.LogLevel.Error);
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }
}
