using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pot : MonoBehaviour
{
    public static Pot Instance { get; private set; }

    public uint Value => (uint)_bets.Sum(x => x.Value);

    private Dictionary<Player, uint> StageBets { get; } = new();
    private Dictionary<Player, uint> _bets;

    private static Betting Betting => Betting.Instance;
    private static Game Game => Game.Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnEnable()
    {
        Game.EndDealEvent += OnEndDeal;
        Game.GameStageOverEvent += GameStageOverEvent;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
    }

    private void OnDisable()
    {
        Game.EndDealEvent -= OnEndDeal;
        Game.GameStageOverEvent -= GameStageOverEvent;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
    }

    public uint GetWinValue(Player player, IReadOnlyList<Player> winners)
    {        
        uint bank = 0;
        if (winners.Contains(player) == false)
        {
            return bank;
        }

        UpdateBets();
        foreach (KeyValuePair<Player, uint> bet in _bets)
        {
            if (winners.Contains(bet.Key) == false)
            {
                bank += bet.Value / (uint)winners.Count;
            }
            else if (bet.Key == player)
            {
                bank += bet.Value;
            }
        }
        
        return bank;
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        _bets?.Clear();
        StageBets?.Clear();
    }

    private void GameStageOverEvent(GameStage gameStage)
    {
        UpdateBets();
        StageBets?.Clear();
    }
        
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        Player player = betActionInfo.Player;
        
        if (StageBets.TryGetValue(player, out uint _) == false)
        {
            StageBets.Add(player, 0);
        }
        
        StageBets[player] = player.BetAmount;
    }

    private void UpdateBets()
    {
        if (_bets == null)
        {
            _bets = new Dictionary<Player, uint>(StageBets);
            return;
        }

        foreach (KeyValuePair<Player,uint> bet in StageBets)
        {
            if (_bets.TryGetValue(bet.Key, out uint _) == false)
            {
                _bets.Add(bet.Key, bet.Value);
            }
            else
            {
                _bets[bet.Key] += bet.Value;
            }
        }
    }
}