using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

/// <summary>
/// BasicUPnP Unity Component, helper for Host or Join
/// </summary>
public class BasicUPnP : MonoBehaviour
{
    /// <summary>
    /// Try to host by using UPnP
    /// </summary>
    /// <param name="ruleName">UPnP rule name (visible on router)</param>
    /// <returns>Async Task, bool = success</returns>
    public async Task<bool> HostUPnP(string ruleName = "Unity-BasicUPnP")
    {
        UnityTransport unityTransport = GetComponent<UnityTransport>();
        if (unityTransport == null)
        {
            Debug.LogError("UPnP: Missing UnityTransport component");
            return false;
        }
        
        int port = unityTransport.ConnectionData.Port;
        
        bool redirectionResult = await UPnP.RedirectExternalConnectionToThisDevice(port, ruleName);
        if (redirectionResult == false)
        {
            Debug.LogError("UPnP: Unable to redirect external connection to this device");
            return false;
        }

        unityTransport.SetConnectionData(unityTransport.ConnectionData.Address, (ushort)port);

        NetworkManager networkManager = GetComponent<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("UPnP: Missing NetworkManager component");
            return false;
        }

        return networkManager.StartHost();
    }

    /// <summary>
    /// Try to connect to host by using direct IP
    /// </summary>
    /// <param name="hostIp">Host router IP</param>
    /// <returns>true if connected</returns>
    public bool JoinUPnP(string hostIp)
    {
        UnityTransport unityTransport = GetComponent<UnityTransport>();
        if (unityTransport == null)
        {
            Debug.LogError("UPnP: Missing UnityTransport component");
            return false;
        }

        unityTransport.SetConnectionData(hostIp, unityTransport.ConnectionData.Port);

        NetworkManager networkManager = GetComponent<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("UPnP: Missing NetworkManager component");
            return false;
        }

        return networkManager.StartClient();
    }

    /// <summary>
    /// Try to host with Unity Relay
    /// </summary>
    /// <param name="maxConnection">number of max client authorized</param>
    /// <returns>async Task, bool = success, string relayId</returns>
    public async Task<(bool, string)> HostRelay(int maxConnection = 99)
    {
        UnityTransport unityTransport = GetComponent<UnityTransport>();
        if (unityTransport == null)
        {
            Debug.LogError("Relay: Missing UnityTransport component");
            return (false, "");
        }

        NetworkManager networkManager = GetComponent<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("Relay: Missing NetworkManager component");
            return (false, "");
        }

        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            return (false, "");
        }

        Allocation allocation;
        string joinCode;

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            return (false, "");
        }

        RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");
        unityTransport.SetHostRelayData(dtlsEndpoint.Host, (ushort)dtlsEndpoint.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, true);

        return (networkManager.StartHost(), joinCode);
    }

    /// <summary>
    /// Try to join a Unity Relay
    /// </summary>
    /// <param name="joinCode">RelayId used to etablish the connection</param>
    /// <returns>async Task, bool = success</returns>
    public async Task<bool> JoinRelay(string joinCode)
    {
        UnityTransport unityTransport = GetComponent<UnityTransport>();
        if (unityTransport == null)
        {
            Debug.LogError("Relay: Missing UnityTransport component");
            return false;
        }

        NetworkManager networkManager = GetComponent<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("Relay: Missing NetworkManager component");
            return false;
        }

        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }

        JoinAllocation allocation;

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }

        RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");
        unityTransport.SetClientRelayData(dtlsEndpoint.Host, (ushort)dtlsEndpoint.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData, true);

        return networkManager.StartClient();
    }
}
