using Unity.Netcode;

public struct WinnerInfo : INetworkSerializeByMemcpy
{
    public readonly ulong WinnerId;
    public readonly uint Chips;

    public WinnerInfo(ulong winnerId, uint chips)
    {
        WinnerId = winnerId;
        Chips = chips;
    }
}
