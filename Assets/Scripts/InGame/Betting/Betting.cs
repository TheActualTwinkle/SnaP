using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Betting : NetworkBehaviour 
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
    
    private const float DelayBeforeStartBet = 1f;
    private const float DelayBeforeEndBet = 0.7f;
    
    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    
    // This field is for CLIENTS. It`s tracking when Server/Host calls the 'EndBetCountdownClientRpc' so when it`s called sets true and routine ends. 
    [ReadOnly] [SerializeField] private bool _isCountdownCoroutineOver; 
    
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
        _isCountdownCoroutineOver = false;
        if (IsServer == false)
        {
            yield return new WaitWhile(() => _isCountdownCoroutineOver == false);
            yield break;
        }
        
        yield return new WaitForSeconds(DelayBeforeStartBet);

        StartBetCountdownClientRpc(player.OwnerClientId);

        while (player.BetAction == BetAction.Empty && _timePaasedSinceBetStart < _betTime)
        {
            _timePaasedSinceBetStart += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        uint betAmount = 0;
        switch (player.BetAction)
        {
            case BetAction.Call:
                betAmount = MaxCallAmount - player.BetAmount;
                break;
            case BetAction.Raise:
                // TODO: Print 'Raise' slider. And setup betAmount.
                betAmount = 20;
                break;
            case BetAction.Bet:
                // TODO: Print 'Raise' slider. And setup betAmount.
                betAmount = 50;
                break;
        }

        BetClientRpc(player.OwnerClientId, betAmount);
        
        yield return new WaitForSeconds(DelayBeforeEndBet);

        EndBetCountdownClientRpc(player.OwnerClientId, player.BetAction, player.BetAmount);
    }

    #region RPC

    [ClientRpc]
    private void StartBetCountdownClientRpc(ulong playerId)
    {
        Player player = PlayerSeats.Players.Find(x => x != null && x.OwnerClientId == playerId);
        
        CurrentBetter = player;
        _timePaasedSinceBetStart = 0f;
        
        PlayerStartBettingEvent?.Invoke(player);
    }

    [ClientRpc]
    private void EndBetCountdownClientRpc(ulong playerId, BetAction betAction, uint betAmount)
    {
        _isCountdownCoroutineOver = true;
        
        Player player = PlayerSeats.Players.Find(x => x != null && x.OwnerClientId == playerId);
        BetActionInfo betActionInfo = new(player, betAction, betAmount);
        
        Log.WriteToFile($"Player ('{player.NickName}'); {betActionInfo.BetAction}; {betAmount}");
        PlayerEndBettingEvent?.Invoke(betActionInfo);
    }

    [ClientRpc]
    private void BetClientRpc(ulong playerId, uint betAmount)
    {
        Player player = PlayerSeats.Players.Find(x => x != null && x.OwnerClientId == playerId);
        
        if (betAmount > 0)
        {
            player.TryBet(betAmount);
        }
    }
    
    #endregion
}