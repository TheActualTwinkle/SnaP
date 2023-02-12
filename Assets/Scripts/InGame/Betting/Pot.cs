using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pot : MonoBehaviour
{
    public static Pot Instance { get; private set; }

    public uint Value => _value;
    [ReadOnly] [SerializeField] private uint _value;
    
    private Dictionary<Player, uint> StageBets { get; } = new();
    
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
        
        return 200; // todo
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

    private void OnEndDeal(WinnerInfo winnerInfo)
    {
        _value = 0;
        StageBets.Clear();
    }

    private void GameStageOverEvent(GameStage gameStage)
    {
        _value += (uint)StageBets.Sum(x => x.Value);
        StageBets.Clear();
    }
        
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        StageBets[betActionInfo.Player] = betActionInfo.BetAmount;
    }
}