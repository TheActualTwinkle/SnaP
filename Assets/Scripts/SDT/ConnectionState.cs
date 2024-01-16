namespace SDT
{
    public enum ConnectionState : byte
    {
        Disconnected,
        Connecting,
        Successful,
        Failed,
        Abandoned
    }
}