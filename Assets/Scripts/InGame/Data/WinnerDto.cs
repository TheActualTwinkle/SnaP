using Unity.Netcode;

public struct WinnerDto : INetworkSerializable
{
    public ulong WinnerId => _winnerId;
    private ulong _winnerId;
    
    public uint Chips => _chips;
    private uint _chips;
    
    public string Combination => _combination;
    private string _combination;

    public WinnerDto(ulong winnerId, uint chips, string combination = "")
    {
        _winnerId = winnerId;
        _chips = chips;
        _combination = combination;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _winnerId);
        serializer.SerializeValue(ref _chips);
        serializer.SerializeValue(ref _combination);
    }
}
