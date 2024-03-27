using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;

public static class ConnectionDataFactory
{
    public static async Task<string> Get(NetworkConnectorType type)
    {
        switch (type)
        {
            case NetworkConnectorType.IpAddress:
            case NetworkConnectorType.IpAddressDedicatedServer:
            case NetworkConnectorType.UPnP:
                if (NetworkManager.Singleton.TryGetComponent(out UnityTransport transport) == true)
                {
                    return $"{transport.ConnectionData.Address}:{transport.ConnectionData.Port}";
                }

                Logger.Log("Can`t get transport component from NetworkManager.", Logger.LogLevel.Error);
                return "N/A";
            
            case NetworkConnectorType.UnityRelay:
                return (await RelayService.Instance.GetJoinCodeAsync(UnityRelayNetworkConnector.AllocationId));
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}