using Unity.Netcode;

public struct WinnerInfo : INetworkSerializable
{
    public ulong WinnerId => _winnerId;
    private ulong _winnerId;
    
    public uint Chips => _сhips;
    private uint _сhips;
    
    public string Combination => _combination;
    private string _combination;

    public WinnerInfo(ulong winnerId, uint chips, string combination = "")
    {
        _winnerId = winnerId;
        _сhips = chips;
        _combination = combination;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _winnerId);
        serializer.SerializeValue(ref _сhips);
        serializer.SerializeValue(ref _combination);
    }
}
