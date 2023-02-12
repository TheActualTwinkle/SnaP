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

    public Player LastBetRaiser { get; private set; }
    public Player CurrentBetter { get; private set; }

    public static bool IsAllIn => PlayerSeats.Players.Any(x => x != null && x.BetAction == BetAction.AllIn);
    public static uint CallAmount => PlayerSeats.Players.Where(x => x != null).Select(x => x.BetAmount).Max();

    private IEnumerator _startBetCountdownCoroutine;
    
    public uint BigBlind => _bigBlind;
    [SerializeField] private uint _bigBlind;

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
        return betAmount < CallAmount ? BetSituation.CallOrFold : BetSituation.CanCheck;
    }
    
    public static uint GetAllInBetAmount(Player player)
    {
        return PlayerSeats.Players.Where(x => x != null).Select(x => x.Stack + x.BetAmount).Min() - player.BetAmount;
    }
    
    public IEnumerator AutoBetBlinds(Player player1, Player player2)
    {
        player1.TryBet(_smallBlind);    
        player2.TryBet(_bigBlind);

        yield return new WaitForSeconds(DelayBeforeEndBet);

        EndBetCountdownClientRpc(player1.OwnerClientId, BetAction.Bet, _smallBlind);
        EndBetCountdownClientRpc(player2.OwnerClientId, BetAction.Bet, _bigBlind);
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
        CurrentBetter = null;
        LastBetRaiser = null;
        
        if (_startBetCountdownCoroutine != null)
        {
            StopCoroutine(_startBetCountdownCoroutine);
        }

        Player player = PlayerSeats.Players.FirstOrDefault(x => x != null && x.OwnerClientId == winnerInfo.WinnerId);
        if (player == null)
        {
            return;
        }

        PlayerEndBettingEvent?.Invoke(new BetActionInfo(player, BetAction.Cancel, 0));
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

        while (player.BetAction is (BetAction.Empty or BetAction.Cancel or BetAction.CallAny or BetAction.CheckFold) && _timePaasedSinceBetStart < _betTime)
        {
            _timePaasedSinceBetStart += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        uint betAmount = 0;
        switch (player.BetAction)
        {
            case BetAction.Call:
                betAmount = CallAmount < player.Stack + player.BetAmount ? CallAmount - player.BetAmount : player.Stack;
                break;
            
            case BetAction.Bet:
            case BetAction.Raise:
                betAmount = player.BetInputFieldValue;
                break;
            
            case BetAction.AllIn:
                betAmount = GetAllInBetAmount(player);
                break;
        }
        
        BetClientRpc(player.OwnerClientId, player.BetAction, betAmount);
        
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
        
        Log.WriteToFile($"Player ('{player.NickName}') id: '{player.OwnerClientId}'; {betAction}; {betAmount}");
        PlayerEndBettingEvent?.Invoke(betActionInfo);
    }

    [ClientRpc]
    private void BetClientRpc(ulong playerId, BetAction betAction, uint betAmount)
    {
        if (betAction is not (BetAction.Bet or BetAction.Raise or BetAction.Call or BetAction.AllIn))
        {
            return;
        }
        
        Player player = PlayerSeats.Players.Find(x => x != null && x.OwnerClientId == playerId);
        player.TryBet(betAmount);
        
        if (Game.CurrentGameStage == GameStage.River && betAction != BetAction.Call)
        {
            LastBetRaiser = player;
        }
    }

    #endregion
}