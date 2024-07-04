using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class Pot : NetworkBehaviour
{
    public static Pot Instance { get; private set; }

    public NetworkVariable<uint> ValueNetworkVariable { get; } = new();

    private readonly Dictionary<Player, uint> _stageBets = new();
    private Dictionary<Player, uint> _bets = new();

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

    private void OnEndDeal(WinnerDto[] winnerInfo)
    {
        _bets?.Clear();
        _stageBets?.Clear();

        if (IsServer == false)
        {
            return;
        }

        ValueNetworkVariable.Value = 0;
    }

    private void GameStageOverEvent(GameStage gameStage)
    {
        UpdateBets();
        _stageBets?.Clear();
    }
        
    private void OnPlayerEndBetting(BetActionInfo betActionInfo)
    {
        Player player = betActionInfo.Player;
        
        if (_stageBets.TryGetValue(player, out uint _) == false)
        {
            _stageBets.Add(player, 0);
        }
        
        _stageBets[player] = player.BetAmount;
    }

    private void UpdateBets()
    {
        if (_bets == null)
        {
            _bets = new Dictionary<Player, uint>(_stageBets);
        }
        else
        {
            foreach (KeyValuePair<Player,uint> bet in _stageBets)
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

        if (IsServer == false)
        {
            return;
        }

        ValueNetworkVariable.Value = (uint)_bets.Sum(x => x.Value);
    }
}