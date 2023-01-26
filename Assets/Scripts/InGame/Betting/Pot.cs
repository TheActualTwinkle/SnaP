using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : MonoBehaviour
{
    public static Pot Instance { get; private set; }

    public Dictionary<Player, uint> Bets { get; } = new();
    
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

    public uint GetWinValue(Player player)
    {
        return 10; // todo
    }

    private void OnEnable()
    {
        Game.EndDealEvent += OnEndDeal;
        Betting.PlayerEndBettingEvent += OnPlayerEndBetting;
    }

    private void OnDisable()
    {
        Game.EndDealEvent -= OnEndDeal;
        Betting.PlayerEndBettingEvent -= OnPlayerEndBetting;
    }

    private void OnEndDeal(WinnerInfo winnerInfo)
    {
        Bets.Clear();
    }
    
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (Bets.ContainsKey(betActionInfo.Player) == false)
        {
            Bets.Add(betActionInfo.Player, betActionInfo.BetAmount);
            return;
        }

        Bets[betActionInfo.Player] += betActionInfo.BetAmount;
    }
}
