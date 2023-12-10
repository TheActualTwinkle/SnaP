using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDT;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UPnPNetworkConnector : INetworkConnector
{
    public IEnumerable<string> ConnectionData { get; private set; } // TODO: Make struct for it: LocalIpAddress, PublicIpAddress, Port, JoinCode!!!
    
    private string _localIpAddress;
    private ushort _port;
    private string _ruleName;
    
    // This is fake-Init.
    // Real Init is in TryCreateGame() method.
    public Task Init()
    {
        ConnectionData = new[] {"Some address via UPnP"};
        return Task.CompletedTask;
    }

    public async Task<bool> TryCreateGame()
    {
        #region Init

        if (ConnectionDataPresenter.TryGetAvailablePort(out ushort port) == false)
        {
            Logger.Log($"Can`t find available port.", Logger.LogLevel.Error);
            return false;
        }
        
        Logger.Log($"Available port set to {port}.");
        string localIpAddress = await ConnectionDataPresenter.GetLocalIpAddressAsync();
        
        string ruleName = "SnaP-UPnP-" + port;

        #endregion
        
        if (NetworkManager.Singleton.IsListening == true)
        {
            Logger.Log("Can`t create game: NetworkManager is already listening.", Logger.LogLevel.Error);
            return false;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        
        bool redirectionResult = await UPnP.RedirectExternalConnectionToThisDevice(port, ruleName);
        if (redirectionResult == false)
        {
            Logger.Log("UPnP: Unable to redirect external connection to this device", Logger.LogLevel.Error);
            return false;
        }

        unityTransport.SetConnectionData(localIpAddress, port);

        NetworkManager.Singleton.StartHost();
        
        Logger.Log("Forwarding to public IP...");
        
        string publicIpAddress = await ConnectionDataPresenter.GetPublicIpAddressAsync();
        ConnectionData = new[] {publicIpAddress, port.ToString()};
        
        NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.Desk, LoadSceneMode.Single);

        return true;
    }

    public async Task<bool> TryJoinGame()
    {
        if (NetworkManager.Singleton.IsListening == true)
        {
            Logger.Log("Can`t join game: NetworkManager is already listening.", Logger.LogLevel.Error);
            return false;
        }
        
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        
        LobbyInfo selectedLobbyInfo = LobbyListCell.SelectedLobbyInfo;

        string selfPublicIP = await ConnectionDataPresenter.GetPublicIpAddressAsync();
        if (selfPublicIP == selectedLobbyInfo.PublicIpAddress)
        {
            Logger.Log("Seems like you and your host using same network. Redirecting to local IP...", Logger.LogLevel.Warning);
            selectedLobbyInfo.PublicIpAddress = await ConnectionDataPresenter.GetLocalIpAddressAsync();
        }
        
        unityTransport.SetConnectionData(selectedLobbyInfo.PublicIpAddress, selectedLobbyInfo.Port);

        ConnectionData = new[] {selectedLobbyInfo.PublicIpAddress, selectedLobbyInfo.Port.ToString()};
        
        NetworkManager.Singleton.StartClient();

        return true;
    }
}
