namespace SDT
{
    public record LobbyInfo
    {
        public int MaxSeats;
        public int PlayersCount;

        public string LobbyName;

        public LobbyInfo(int maxSeats, int playersCount, string lobbyName)
        {
            MaxSeats = maxSeats;
            PlayersCount = playersCount;
            LobbyName = lobbyName;
        }
        
        public void Deconstruct(out int maxSeats, out int playersCount, out string lobbyName)
        {
            maxSeats = MaxSeats;
            playersCount = PlayersCount;
            lobbyName = LobbyName;
        }
    }
}