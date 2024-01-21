namespace SDT
{
    public enum ConnectionState : byte
    {
        Disconnected,
        DisconnectedPortClosed,
        Connecting,
        Successful,
        Failed,
        Abandoned
    }
}