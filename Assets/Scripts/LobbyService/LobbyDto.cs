using System.Net;
using JetBrains.Annotations;

namespace LobbyService
{
    public class LobbyDto
    {
        public string PublicIpAddress;
        public ushort Port;
        
        public int MaxSeats;
        public int PlayersCount;

        public string LobbyName;

        public LobbyDto(string publicIpAddress, ushort port, int maxSeats, int playersCount, string lobbyName)
        {
            PublicIpAddress = publicIpAddress;
            Port = port;
            MaxSeats = maxSeats;
            PlayersCount = playersCount;
            LobbyName = lobbyName;
        }
        
        [UsedImplicitly]
        public void Deconstruct(ref LobbyDto lobbyInfo)
        {
            lobbyInfo.PublicIpAddress = PublicIpAddress;
            lobbyInfo.Port = Port;
            lobbyInfo.MaxSeats = MaxSeats;
            lobbyInfo.PlayersCount = PlayersCount;
            lobbyInfo.LobbyName = LobbyName;
        }
    }
}