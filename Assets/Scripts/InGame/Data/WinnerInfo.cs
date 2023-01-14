using Unity.Netcode;

public struct WinnerData : INetworkSerializeByMemcpy
{
    public readonly ulong WinnerId;
    public readonly uint Chips;

    public WinnerData(ulong winnerId, uint chips)
    {
        WinnerId = winnerId;
        Chips = chips;
    }
}
