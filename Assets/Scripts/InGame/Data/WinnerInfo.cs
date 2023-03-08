using Unity.Netcode;

public struct WinnerInfo : INetworkSerializeByMemcpy
{
    public readonly ulong WinnerId;
    public readonly uint Chips;
    public readonly string Combination;

    public WinnerInfo(ulong winnerId, uint chips, string combination = "null")
    {
        WinnerId = winnerId;
        Chips = chips;
        Combination = combination;
    }
}
