using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Open.Nat;

namespace SDT
{
    /// <summary>
    /// Receive some bytes from SDT Server to check if the port is open.
    /// </summary>
    public static class UdpPortChecker
    {
        public static async Task<bool> Check(ushort port)
        {
            IPAddress localIpAddress = await ConnectionDataPresenter.GetLocalIpAddressAsync();
            IPEndPoint endPoint = new(localIpAddress, port);
            IEnumerable<Mapping> matchingMappings = await UPnP.GetMatchingMappingsAsync(endPoint);

            return matchingMappings.Any();
        }
    }
}