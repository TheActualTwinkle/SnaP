using System;
using System.Net;

namespace LobbyService.TcpIp
{
    public class LobbyServiceInfo
    {
        public LobbyServiceInfo(IPEndPoint serversEndPoint, IPEndPoint clientsEndPoint, TimeSpan serverLobbyInitializationInterval)
        {
            ServersEndPoint = serversEndPoint;
            ClientsEndPoint = clientsEndPoint;

            ServerLobbyInitializationInterval = serverLobbyInitializationInterval;
        }
        
        public IPEndPoint ClientsEndPoint { get; }
        
        
        public IPEndPoint ServersEndPoint { get; }
        public TimeSpan ServerLobbyInitializationInterval { get; }
    }
}