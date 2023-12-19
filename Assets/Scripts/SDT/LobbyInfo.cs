namespace SDT
{
    public record LobbyInfo
    {
        public string PublicIpAddress;
        public ushort Port;
        
        public int MaxSeats;
        public int PlayersCount;

        public string LobbyName;

        public LobbyInfo(string publicIpAddress, ushort port, int maxSeats, int playersCount, string lobbyName)
        {
            PublicIpAddress = publicIpAddress;
            Port = port;
            MaxSeats = maxSeats;
            PlayersCount = playersCount;
            LobbyName = lobbyName;
        }
        
        public void Deconstruct(ref LobbyInfo lobbyInfo)
        {
            lobbyInfo.PublicIpAddress = PublicIpAddress;
            lobbyInfo.Port = Port;
            lobbyInfo.MaxSeats = MaxSeats;
            lobbyInfo.PlayersCount = PlayersCount;
            lobbyInfo.LobbyName = LobbyName;
        }
    }
}