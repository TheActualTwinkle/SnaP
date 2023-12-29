using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace SDT
{
    /// <summary>
    /// Sends some bytes to SDT Server to check if the port is open.
    /// </summary>
    public static class UdpPortChecker
    {
        public static async Task<bool> Check(uint timeoutMs)
        {
            string ip = await ConnectionDataPresenter.GetLocalIpAddressAsync();
            
            UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            ushort port = unityTransport.ConnectionData.Port;

            try
            {
                UdpClient udpClient = new(ip, port);

                CancellationTokenSource cancellationToken = new((int)timeoutMs);
            
                Memory<byte> buffer = new(new byte[2048]);
                await udpClient.ReceiveAsync(cancellationToken.Token);

                if (udpReceiveResult.Buffer.Length <= 0)
                {
                    return false;
                }

                Logger.Log("Successfully sent port forward check message to SDT Server.", Logger.LogSource.SnaPDataTransfer);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log($"Can`t check if port {port} is visible.", Logger.LogLevel.Error, Logger.LogSource.SnaPDataTransfer);
                return false;
            }
        }
    }
}