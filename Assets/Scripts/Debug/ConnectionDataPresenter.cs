using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public static class ConnectionDataPresenter
{
    private static IPAddress _publicIpAddress;
    
    public static async Task<IPAddress> GetOrUpdatePublicIpAddressAsync()
    {
        return _publicIpAddress ??= await UPnP.GetExternalIpAsync();
    }
    
    public static async Task<IPAddress> GetLocalIpAddressAsync()
    {
        return (await Dns.GetHostEntryAsync(Dns.GetHostName()))
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
    }

    public static ushort GetGamePort()
    {
        if (NetworkManager.Singleton.TryGetComponent(out UnityTransport transport) == false)
        {
            throw new ArgumentException($"Can`t get transport component from NetworkManager.");
        }

        return transport.ConnectionData.Port;
    }

    public static bool TryGetAvailableUdpPort(out ushort port)
    {
        const ushort minPort = 10000; 
        const ushort maxPort = ushort.MaxValue - 1;
        
        IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
        HashSet<int> usedPorts = Enumerable.Empty<int>()
            .Concat(ipProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint.Port))
            .Concat(ipProperties.GetActiveTcpListeners().Select(l => l.Port)) // TODO: Can be removed? Cause of TCP ignoring.
            .Concat(ipProperties.GetActiveUdpListeners().Select(l => l.Port))
            .ToHashSet();
        
        for (ushort i = minPort; i <= maxPort; i++)
        {
            if (usedPorts.Contains(i) == true)
            {
                continue;
            }

            port = i;
            return true;
        }

        port = 0;
        return false;
    }
}
