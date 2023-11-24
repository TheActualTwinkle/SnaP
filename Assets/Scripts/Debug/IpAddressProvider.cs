using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

public static class IpAddressProvider
{
    public static async Task<string> GetPublicAsync()
    {
        return (await new HttpClient().GetStringAsync("http://icanhazip.com"))
            .Replace("\\r\\n", "").Replace("\\n", "").Trim();
    }

    public static async Task<string> GetLocalAsync()
    {
        return (await Dns.GetHostEntryAsync(Dns.GetHostName()))
            .AddressList.First(
            f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }
}
