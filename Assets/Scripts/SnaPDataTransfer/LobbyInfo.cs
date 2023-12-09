namespace SDT
{
    public record LobbyInfo
    {
        public string IpAddress;
        public ushort Port;
        
        public int MaxSeats;
        public int PlayersCount;

        public string LobbyName;

        public LobbyInfo(string ipAddress, ushort port, int maxSeats, int playersCount, string lobbyName)
        {
            IpAddress = ipAddress;
            Port = port;
            MaxSeats = maxSeats;
            PlayersCount = playersCount;
            LobbyName = lobbyName;
        }
        
        public void Deconstruct(ref LobbyInfo lobbyInfo)
        {
            lobbyInfo.IpAddress = IpAddress;
            lobbyInfo.Port = Port;
            lobbyInfo.MaxSeats = MaxSeats;
            lobbyInfo.PlayersCount = PlayersCount;
            lobbyInfo.LobbyName = LobbyName;
        }
    }
}