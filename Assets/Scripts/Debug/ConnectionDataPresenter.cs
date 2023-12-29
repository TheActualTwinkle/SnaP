using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

public static class ConnectionDataPresenter
{
    public const ushort SnaPDefaultPort = 47924;
    
    private static string _publicIpAddress;
    
    public static async Task<string> GetPublicIpAddressAsync()
    {
        if (string.IsNullOrEmpty(_publicIpAddress) == true)
        {
            _publicIpAddress = (await UPnP.GetExternalIp()).ToString();
        }
        
        return _publicIpAddress;
    }

    public static async Task<string> GetLocalIpAddressAsync()
    {
        return (await Dns.GetHostEntryAsync(Dns.GetHostName()))
            .AddressList.First(
            f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
    
    public static async Task<List<string>> GetLocalIpAddressesAsync()
    {
        return (await Dns.GetHostEntryAsync(Dns.GetHostName()))
            .AddressList.Select(x => x.ToString()).ToList();
    }
    
    public static bool TryGetAvailablePort(out ushort port)
    {
        const ushort lowerPort = 10000; 
        const ushort upperPort = ushort.MaxValue - 1;
        
        IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
        HashSet<int> usedPorts = Enumerable.Empty<int>()
            .Concat(ipProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint.Port))
            .Concat(ipProperties.GetActiveTcpListeners().Select(l => l.Port)) // TODO: Can be removed? Cause of TCP ignoring.
            .Concat(ipProperties.GetActiveUdpListeners().Select(l => l.Port))
            .ToHashSet();
        
        for (ushort i = lowerPort; i <= upperPort; i++)
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
