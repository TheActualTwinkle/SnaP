public struct BetActionInfo
{
    public readonly Player Player;
    public readonly BetAction BetAction;
    public readonly uint BetAmount;

    public BetActionInfo(Player player, BetAction betAction, uint betAmount)
    {
        Player = player;
        BetAction = betAction;
        BetAmount = betAmount;
    }
}
