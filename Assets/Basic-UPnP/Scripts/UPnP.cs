using UnityEngine;
using Open.Nat;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;

/// <summary>
/// Static helper class for UPnP
/// </summary>
public static class UPnP
{
	/// <summary>
	/// Try to get External IP (provided by FAI), pass this IP to Client
	/// </summary>
	/// <returns>Async Task, IPAddress</returns>
	public static async Task<IPAddress> GetExternalIp()
	{
		NatDevice router = await GetInterDevice();

		return await router.GetExternalIPAsync();
	}

	/// <summary>
	/// Try to create a n UPnP rule on the router
	/// </summary>
	/// <param name="port">Port to redirect to the host device</param>
	/// <param name="ruleName">Rule name displayed on Router interface</param>
	/// <returns>Async Task, Bool = success</returns>
	public static async Task<bool> RedirectExternalConnectionToThisDevice(int port, string ruleName)
	{
		try
		{
			NatDevice router = await GetInterDevice();

			if (router != null)
			{
				await router.CreatePortMapAsync(new Mapping(Protocol.Udp, port, port, ruleName));
			}
			else
			{
				return false;
			}
		}
        catch (MappingException e)
        {
            Debug.LogError(e.Message);
            return false;
        }

		return true;
	}

	/// <summary>
	/// try to retreive a UPnP compatible Device on the Route
	/// </summary>
	/// <returns>Async Task, NatDevice</returns>
	private static async Task<NatDevice> GetInterDevice()
	{
		NatDiscoverer discoverer = new NatDiscoverer();
		CancellationTokenSource cts = new CancellationTokenSource(10000);
		List<NatDevice> devices = new List<NatDevice>(await discoverer.DiscoverDevicesAsync(PortMapper.Upnp, cts));
		foreach (NatDevice device in devices)
		{
			if (device.LocalAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
			{
				return device;
			}
		}

		return null;
	}
}