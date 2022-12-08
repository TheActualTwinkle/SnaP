using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Betting : MonoBehaviour 
{
    public static Betting Instance { get; private set; }
    
    public event Action<Player> PlayerStartBettingEvent;
    public event Action<Player, BetAction> PlayerEndBettingEvent;

    public Player CurrentBetter { get; private set; }

    private static uint CallAmount => PlayerSeats.Instance.Players.Where(x => x != null).Select(x => x.BetAmount).Max();

    public uint BigBlind => _bigBlind;
    [ReadOnly] [SerializeField] private uint _bigBlind;
    
    public uint SmallBlind => _smallBlind;
    [SerializeField] private uint _smallBlind;
    
    public float BetTime => _betTime;
    [SerializeField] private float _betTime;
    [SerializeField] [ReadOnly] private float _timePaasedSinceBetStart;
    
    // private const float DelayBeforeStarBet = 1f;
    
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
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
    }

    private void OnDisable()
    {
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
    }

    public static BetSituation GetBetSituation(uint betAmount)
    {
        return betAmount <= CallAmount ? BetSituation.CallEqualsOrLessCheck : BetSituation.CallGreaterCheck;
    }
    
    public IEnumerator StartBetCountdown(Player player)
    {
        CurrentBetter = player;
        PlayerStartBettingEvent?.Invoke(player);
        
        // yield return new WaitForSeconds(DelayBeforeStarBet);
        
        _timePaasedSinceBetStart = 0f;
        while (player.ChoosenBetAction == BetAction.Empty && _timePaasedSinceBetStart < _betTime)
        {
            _timePaasedSinceBetStart += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        PlayerEndBettingEvent?.Invoke(player, player.ChoosenBetAction);
    }

    private void OnPlayerLeave(Player player, int seatIndex)
    {
        if (player != CurrentBetter)
        {
            return;
        }

        _timePaasedSinceBetStart = _betTime;
        PlayerEndBettingEvent?.Invoke(player, player.ChoosenBetAction);
    }
}