namespace SDT
{
    public enum ConnectionState : byte
    {
        Disconnected,
        DisconnectedPortClosed,
        Connecting,
        Successful,
        Failed,
        
        // Can be used if SDT.Client | SDT.Server is destroyed somehow.
        Abandoned
    }
}