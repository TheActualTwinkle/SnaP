using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDT;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UPnPNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData { get; private set; } // TODO: Make struct for it: LocalIpAddress, PublicIpAddress, Port, JoinCode!!!
    
    private string _localIpAddress;
    private ushort _port;
    private string _ruleName;
    
    public async Task Init()
    {
        if (ConnectionDataPresenter.TryGetAvailablePort(out _port) == false)
        {
            Logger.Log($"Can`t find available port.", Logger.LogLevel.Error);
            return;
        }
        
        Logger.Log($"Available port set to {_port}.");
        _localIpAddress = await ConnectionDataPresenter.GetLocalIpAddressAsync();
        
        _ruleName = "SnaP-UPnP-" + _port;
        
        ConnectionData = new[] {_localIpAddress, _port.ToString()};
    }

    public async Task<bool> TryCreateGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            Logger.Log("Can`t create game: NetworkManager is already listening.", Logger.LogLevel.Error);
            return false;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        unityTransport.SetConnectionData(_localIpAddress, _port);

        bool redirectionResult = await UPnP.RedirectExternalConnectionToThisDevice(_port, _ruleName);
        if (redirectionResult == false)
        {
            Logger.Log("UPnP: Unable to redirect external connection to this device", Logger.LogLevel.Error);
            return false;
        }

        unityTransport.SetConnectionData(unityTransport.ConnectionData.Address, _port);

        NetworkManager.Singleton.StartHost();
        
        Logger.Log("Forwarding to public IP...");
        
        string publicIpAddress = await ConnectionDataPresenter.GetPublicIpAddressAsync();
        ConnectionData = new[] {publicIpAddress, _port.ToString()};
        
        NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.Desk, LoadSceneMode.Single);

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

        LobbyInfo selectedLobbyInfo = LobbyListCell.SelectedLobbyInfo;
        unityTransport.SetConnectionData(selectedLobbyInfo.IpAddress, selectedLobbyInfo.Port);

        NetworkManager.Singleton.StartClient();

        return Task.FromResult(true);
    }
}
