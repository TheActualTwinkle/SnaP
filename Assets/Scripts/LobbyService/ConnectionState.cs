namespace LobbyService
{
    public enum ConnectionState : byte
    {
        Disconnected,
        DisconnectedPortClosed,
        Connecting,
        Successful,
        Failed
    }
}