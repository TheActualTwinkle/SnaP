using System;
using System.Net;
using LobbyService.Grpc;
using LobbyService.Interfaces;
using LobbyService.TcpIp;
using Zenject;

public class LobbyServiceInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        IPAddress sdtAddress = IPAddress.Parse("90.189.208.37");
        const ushort serversPort = 47920;
        const ushort clientsPort = 47921;

        LobbyServiceInfo lobbyServiceInfo = new(
            new IPEndPoint(sdtAddress, serversPort),
            new IPEndPoint(sdtAddress, clientsPort),
            TimeSpan.FromSeconds(1)
        );
        
        // gRPC. TODO: https://github.com/TheActualTwinkle/SnaP/issues/50
        // IServersLobbyService serversLobbyService = new ServersGrpcLobbyService(lobbyServiceInfo.ServersEndPoint);
        // IClientsLobbyService clientsLobbyService = new ClientsGrpcLobbyService(lobbyServiceInfo.ClientsEndPoint);
        
        IServersLobbyService serversLobbyService = new ServersTcpIpLobbyService(lobbyServiceInfo);
        IClientsLobbyService clientsLobbyService = new ClientsTcpIpLobbyService(lobbyServiceInfo);
        
        Container.Bind<IServersLobbyService>().FromInstance(serversLobbyService).AsSingle();
        Container.Bind<IClientsLobbyService>().FromInstance(clientsLobbyService).AsSingle();
    }
}