using System;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace SDT
{
    /// <summary>
    /// Sends some bytes to SDT Server to check if the port is open.
    /// </summary>
    public class UdpPortChecker : MonoBehaviour
    {
        private async void Start()
        {
            // If this is a client, then destroy this object.
            if (NetworkManager.Singleton.IsServer == false)
            {
                Destroy(gameObject);
                return;
            }
            
            UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            string ip = unityTransport.ConnectionData.Address;
            ushort port = unityTransport.ConnectionData.Port;
            
            UdpClient udpClient = new(ip, port);
            
            
            await udpClient.SendAsync(new byte[]{48}, 1);
            
            Logger.Log("Successfully sent port forward check message to SDT Server.", Logger.LogSource.SnaPDataTransfer);
            Destroy(gameObject);
        }
    }
}