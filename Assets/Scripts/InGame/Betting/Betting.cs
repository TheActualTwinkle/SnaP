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
    private readonly NetworkVariable<ulong> _currentBetterId = new(NullBetterId);

    public static bool IsAllIn => PlayerSeats.Players.Any(x => x != null && x.BetAction == BetAction.AllIn);
    public static uint CallAmount => PlayerSeats.Players.Where(x => x != null).Select(x => x.BetAmount).Max();

    private IEnumerator _startBetCountdownCoroutine;
    
    public uint BigBlind => _bigBlind;
    [SerializeField] private uint _bigBlind;

    public uint SmallBlind => _smallBlind;
    [SerializeField] private uint _smallBlind;

    public float BetTime => _betTime;
    [SerializeField] private float _betTime;

    public float TimePassedSinceBetStart => _timePassedSinceBetStart.Value;
    public NetworkVariable<float> _timePassedSinceBetStart = new();
    
    private const float DelayBeforeStartBet = 1f;
    private const float DelayBeforeEndBet = 0.7f;
    private const ulong NullBetterId = ulong.MaxValue;

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

    private IEnumerator Start()
    {        
        if (IsOwner == true)
        {
            yield break;
        }

        if (Game.IsPlaying == false)
        {
            yield break;
        }

        yield return new WaitUntil(() => _currentBetterId.Value != NullBetterId);
        
        Player player = PlayerSeats.Players.FirstOrDefault(x => x != null && x.OwnerClientId == _currentBetterId.Value);
        if (player == null)
        {
            yield break;
        }
        
        PlayerStartBettingEvent?.Invoke(player);
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
        BetAction betAction = BetAction.Bet;
        
        if (player1.Stack <= _smallBlind || player2.Stack <= _bigBlind)
        {
            var betValue = (uint)Mathf.Min((int)player1.Stack, (int)player2.Stack);
            player1.TryBet(betValue);
            player2.TryBet(betValue);

            betAction = BetAction.AllIn;
        }
        else
        {
            player1.TryBet(_smallBlind);
            player2.TryBet(_bigBlind);
        }
        
        yield return new WaitForSeconds(DelayBeforeEndBet / 4);
        yield return new WaitUntil(() => (player1.BetAmount == _smallBlind && player2.BetAmount == _bigBlind) || betAction == BetAction.AllIn);

        EndBetCountdownClientRpc(player1.OwnerClientId, betAction, player1.BetAmount);
        EndBetCountdownClientRpc(player2.OwnerClientId, betAction, player2.BetAmount);
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
        
        if (IsServer == true)
        {
            SetTimePassedSinceBetStartValueServerRpc(_betTime);
        }
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        if (IsServer == true)
        {
            SetCurrentBetterIdValueServerRpc(NullBetterId);
        }
        
        CurrentBetter = null;
        LastBetRaiser = null;
        
        if (_startBetCountdownCoroutine != null)
        {
            StopCoroutine(_startBetCountdownCoroutine);
        }

        Player player = PlayerSeats.Players.FirstOrDefault(x => x != null && winnerInfo.Select(info => info.WinnerId).Contains(x.OwnerClientId));
        if (player == null)
        {
            return;
        }

        PlayerEndBettingEvent?.Invoke(new BetActionInfo(player, BetAction.Cancel, 0));
    }
    
    private IEnumerator StartBetCountdown(Player player)
    {
        yield return new WaitForSeconds(DelayBeforeStartBet);

        StartBetCountdownClientRpc(player.OwnerClientId);

        while (player.BetAction is (BetAction.Empty or BetAction.Cancel or BetAction.CallAny or BetAction.CheckFold) && _timePassedSinceBetStart.Value < _betTime)
        {
            if (player.SeatNumber == Player.NullSeatNumber)
            {
                EndBetCountdownClientRpc(player.OwnerClientId, BetAction.Empty, player.BetAmount);
                yield break;
            }
            
            SetTimePassedSinceBetStartValueServerRpc(_timePassedSinceBetStart.Value + Time.deltaTime);
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
        
        uint totalBet = betAmount + player.BetAmount; // Ping compensation.
        BetAction betAction = player.BetAction; // Ping compensation.
        BetClientRpc(player.OwnerClientId, betAction, betAmount);

        yield return new WaitForSeconds(DelayBeforeEndBet);
        yield return new WaitUntil(() => player.BetAmount == totalBet);

        EndBetCountdownClientRpc(player.OwnerClientId, betAction, player.BetAmount);
    }

    #region RPC

    [ServerRpc]
    private void SetTimePassedSinceBetStartValueServerRpc(float value)
    {
        _timePassedSinceBetStart.Value = value;
    }

    [ServerRpc]
    private void SetCurrentBetterIdValueServerRpc(ulong value)
    {
        _currentBetterId.Value = value;
    }

    [ClientRpc]
    private void StartBetCountdownClientRpc(ulong playerId)
    {
        Player player = PlayerSeats.Players.Find(x => x != null && x.OwnerClientId == playerId);

        CurrentBetter = player;

        if (IsServer == true)
        {
            SetCurrentBetterIdValueServerRpc(player.OwnerClientId);
        }
        
        PlayerStartBettingEvent?.Invoke(player);
        
        Log.WriteToFile($"Player ({player}), seat №{PlayerSeats.Players.IndexOf(player)} start betting");
    }

    [ClientRpc]
    private void EndBetCountdownClientRpc(ulong playerId, BetAction betAction, uint betAmount)
    {
        if (IsServer == true)
        {
            SetTimePassedSinceBetStartValueServerRpc(0);
        }

        Player player = PlayerSeats.Players.Find(x => x != null && x.OwnerClientId == playerId);
        if (player == null)
        {
            player = FindObjectsOfType<Player>().FirstOrDefault(x => x.OwnerClientId == playerId);
        }

        if (player == null)
        {
            return;
        }

        BetActionInfo betActionInfo = new(player, betAction, betAmount);
        
        Log.WriteToFile($"Player ({player}); {betAction}; {betAmount}");
        PlayerEndBettingEvent?.Invoke(betActionInfo);

        if (betAction == BetAction.Check)
        {
            SfxAudio.Instance.Play(3);
        }
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