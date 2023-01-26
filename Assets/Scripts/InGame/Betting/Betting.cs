using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Betting : MonoBehaviour 
{
    public static Betting Instance { get; private set; }
    
    public event Action<Player> PlayerStartBettingEvent;
    public event Action<BetActionInfo> PlayerEndBettingEvent;

    public Player CurrentBetter { get; private set; }

    private static uint MaxCallAmount => PlayerSeats.Instance.Players.Where(x => x != null).Select(x => x.BetAmount).Max();

    private IEnumerator _startBetCountdownCoroutine;
    
    public uint BigBlind => _bigBlind;
    [ReadOnly] [SerializeField] private uint _bigBlind;
    
    public uint SmallBlind => _smallBlind;
    [SerializeField] private uint _smallBlind;
    
    public float BetTime => _betTime;
    [SerializeField] private float _betTime;
    [SerializeField] [ReadOnly] private float _timePaasedSinceBetStart;
    
    private const float DelayBeforeStarBet = 1.75f;
    
    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    
    private void OnValidate()
    {
        _bigBlind = _smallBlind * 2;    
    }

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
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
    }

    private void OnDisable()
    {
        Game.EndDealEvent -= OnEndDeal;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
    }

    public static BetSituation GetBetSituation(uint betAmount)
    {
        return betAmount < MaxCallAmount ? BetSituation.CallOrFold : BetSituation.CanCheck;
    }

    public IEnumerator Bet(Player player)
    {        
        if (_startBetCountdownCoroutine != null)
        {
            StopCoroutine(_startBetCountdownCoroutine);
        }
        _startBetCountdownCoroutine = StartBetCountdown(player);
        yield return StartCoroutine(_startBetCountdownCoroutine);
    }

    private void OnPlayerLeave(Player player, int seatIndex)
    {
        if (player != CurrentBetter)  
        {
            return;
        }

        if (_startBetCountdownCoroutine == null)
        {
            return;
        }

        StopCoroutine(_startBetCountdownCoroutine);
        PlayerEndBettingEvent?.Invoke(new BetActionInfo(player, BetAction.Fold, 0));
    }

    private void OnEndDeal(WinnerInfo winnerInfo)
    {
        if (_startBetCountdownCoroutine != null)
        {
            StopCoroutine(_startBetCountdownCoroutine);
        }

        Player player = PlayerSeats.Players.FirstOrDefault(x => x != null && x.OwnerClientId == winnerInfo.WinnerId);
        if (player == null)
        {
            return;
        }

        PlayerEndBettingEvent?.Invoke(new BetActionInfo(player, (BetAction)(-1), 0));
    }
    
    private IEnumerator StartBetCountdown(Player player)
    {
        CurrentBetter = player;
        _timePaasedSinceBetStart = 0f;

        yield return new WaitForSeconds(DelayBeforeStarBet);

        PlayerStartBettingEvent?.Invoke(player);
        
        while (player.ChoosenBetAction == BetAction.Empty && _timePaasedSinceBetStart < _betTime)
        {
            _timePaasedSinceBetStart += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        print("PlayerEndBettingEvent; " + player.ChoosenBetAction + "; " + player.BetAmount);
        PlayerEndBettingEvent?.Invoke(new BetActionInfo(player, player.ChoosenBetAction, player.BetAmount));
    }
}