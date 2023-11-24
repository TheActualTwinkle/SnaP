namespace SnaPDataTransfer
{
    public struct LobbyInfo
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
    }
}