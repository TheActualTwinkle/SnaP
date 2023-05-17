using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    public static Game Instance { get; private set; }

    public event Action<GameStage> GameStageBeganEvent;
    public event Action<GameStage> GameStageOverEvent;
    public event Action<WinnerInfo[]> EndDealEvent;

    public string CodedBoardCardsString => _codedBoardCardsString.Value.ToString();
    private readonly NetworkVariable<FixedString64Bytes> _codedBoardCardsString = new();

    public List<CardObject> BoardCards => _board.Cards.ToList();

    public bool IsPlaying => _isPlaying.Value;
    private readonly NetworkVariable<bool> _isPlaying = new();

    private static Betting Betting => Betting.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Pot Pot => Pot.Instance;
    
    [SerializeField] private BoardButton _boardButton;
    [ReadOnly] [SerializeField] private Board _board;
    private CardDeck _cardDeck;

    private IEnumerator _stageCoroutine;
    private IEnumerator _startDealWhenСonditionTrueCoroutine;
    private IEnumerator _startDealAfterRoundsInterval;

    public GameStage CurrentGameStage => _currentGameStage.Value;
    private readonly NetworkVariable<GameStage> _currentGameStage = new();
    
    private bool ConditionToStartDeal => _isPlaying.Value == false && 
                                         PlayerSeats.PlayersAmount >= 2 && 
                                         PlayerSeats.Players.Where(x => x != null).All(x => x.BetAmount == 0);

    [SerializeField] private float _roundsInterval;
    [SerializeField] private float _showdownEndTime;

    // This fields is for CLIENTS. It`s tracking when Server/Host calls the 'EndStageCoroutineClientRpc' so when it`s called sets true and routine ends.
    private readonly NetworkVariable<bool> _isStageCoroutineOver = new();

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
        PlayerSeats.PlayerSitEvent += OnPlayerSit;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
    }

    private void OnDisable()
    {
        PlayerSeats.PlayerSitEvent -= OnPlayerSit;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
    }

    private IEnumerator StartPreflop()
    {
        if (IsServer == false)
        {
            yield return new WaitWhile(() => _isStageCoroutineOver.Value == false);
            yield break;
        }

        int[] turnSequence = _boardButton.GetTurnSequence();
        foreach (int index in turnSequence)
        {
            Player player = PlayerSeats.Players[index];
            SetPlayersPocketCardsClientRpc(player.OwnerClientId, _cardDeck.PullCard(), _cardDeck.PullCard());
        }
        
        Player player1 = PlayerSeats.Players[turnSequence[0]];
        Player player2 = PlayerSeats.Players[turnSequence[1]];
        yield return Betting.AutoBetBlinds(player1, player2);

        int[] preflopTurnSequence = _boardButton.GetPreflopTurnSequence();

        yield return Bet(preflopTurnSequence);
        
        ChangeIsStageCoroutineOverValueServerRpc(true);
        EndStageClientRpc();

        yield return new WaitForSeconds(_roundsInterval);
        
        StartNextStageClientRpc();
    }

    // Stage like Flop, Turn and River.
    private IEnumerator StartMidGameStage()
    {
        if (IsServer == false)
        {
            yield return new WaitWhile(() => _isStageCoroutineOver.Value == false);
            yield break;
        }
        
        if (Betting.IsAllIn == false)
        {
            int[] turnSequence = _boardButton.GetTurnSequence();
            yield return Bet(turnSequence);
        }

        ChangeIsStageCoroutineOverValueServerRpc(true);

        EndStageClientRpc();

        yield return new WaitForSeconds(_roundsInterval);
        
        StartNextStageClientRpc();
    }

    private IEnumerator StartShowdown()
    {
        if (IsServer == false)
        {
            yield return new WaitWhile(() => _isStageCoroutineOver.Value == false);
            yield break;
        }
        
        int[] turnSequence = _boardButton.GetShowdownTurnSequence();
        
        List<Player> winners = new();
        Hand winnerHand = new();
        for (var i = 0; i < turnSequence.Length; i++)
        {
            Player player = PlayerSeats.Players[turnSequence[i]];
            List<CardObject> completeCards = _board.Cards.ToList();
            completeCards.Add(player.PocketCard1); completeCards.Add(player.PocketCard2);

            Hand bestHand = CombinationСalculator.GetBestHand(new Hand(completeCards));

            if (i == 0 || bestHand > winnerHand)
            {
                winners.Clear();
                winners.Add(player);
                winnerHand = bestHand;
            }
            else if (bestHand == winnerHand)
            {
                winners.Add(player);
            }
        }
        
        if (winners.Count == 0)
        {
            throw new NullReferenceException();
        }
        
        ChangeIsStageCoroutineOverValueServerRpc(true);

        EndStageClientRpc();
        
        yield return new WaitForSeconds(_showdownEndTime);

        List<WinnerInfo> winnerInfo = new();
        foreach (Player winner in winners)
        {
            winnerInfo.Add(new WinnerInfo(winner.OwnerClientId, Pot.GetWinValue(winner, winners), winnerHand.ToString()));
        }

        EndDealClientRpc(winnerInfo.ToArray());
    }

    private IEnumerator Bet(int[] turnSequence)
    {
        if (IsServer == false)
        {
            yield break;
        }
        
        for (var i = 0;; i++)
        {
            foreach (int index in turnSequence)
            {
                Player player = PlayerSeats.Players[index];

                if (player == null)
                {
                    continue;
                }

                yield return Betting.Bet(player);
            
                List<Player> notFoldPlayers = PlayerSeats.Players.Where(x => x != null && x.BetAction != BetAction.Fold).ToList();
                if (notFoldPlayers.Count == 1)
                {
                    ulong winnerId = notFoldPlayers[0].OwnerClientId;
                    WinnerInfo[] winnerInfo = {new(winnerId, Pot.GetWinValue(notFoldPlayers[0], new []{notFoldPlayers[0]}))};
                    EndDealClientRpc(winnerInfo);
                    yield break;
                }

                if (i == 0 || IsBetsEquals() == false)
                {
                    continue;
                }

                yield break;
            }

            if (i != 0 || IsBetsEquals() == false)
            {
                continue;
            }

            yield break;
        }
    }
        
    private void OnPlayerSit(Player player, int seatNumber)
    {
        if (IsServer == false)
        {
            return;
        }
        
        if (_startDealAfterRoundsInterval != null || IsPlaying == true)   
        {
            return;
        }
        
        _startDealAfterRoundsInterval = StartDealAfterRoundsInterval();
        StartCoroutine(_startDealAfterRoundsInterval);
    }

    private void OnPlayerLeave(Player player, int seatNumber)
    {
        if (IsServer == false)
        {
            return;
        }
        
        if (_isPlaying.Value == false)
        {
            return; 
        }

        if (PlayerSeats.Players.Count(x => x != null && x.BetAction != BetAction.Fold) != 1)
        {
            return;
        }
        
        Player winner = PlayerSeats.Players.FirstOrDefault(x => x != null);
        ulong winnerId = winner!.OwnerClientId; 
        WinnerInfo[] winnerInfo = {new(winnerId, Pot.GetWinValue(winner, new []{winner}))};
        EndDealClientRpc(winnerInfo);
    }

    private IEnumerator StartDealAfterRoundsInterval()
    {
        yield return new WaitForSeconds(_roundsInterval);

        PlayerSeats.SitEveryoneWaiting();
        PlayerSeats.KickPlayersWithZeroStack();
        
        if (IsServer == false || _startDealWhenСonditionTrueCoroutine != null)
        {
            yield break;
        }
        
        _startDealWhenСonditionTrueCoroutine = StartDealWhenСonditionTrue();
        yield return StartCoroutine(_startDealWhenСonditionTrueCoroutine);

        _startDealAfterRoundsInterval = null;
    }
    
    private IEnumerator StartDealWhenСonditionTrue()
    {
        yield return new WaitUntil(() => ConditionToStartDeal == true);
        yield return new WaitForSeconds(0.05f);

        _cardDeck = new CardDeck();
        
        StartDealClientRpc(CardObjectConverter.GetCodedCards(_cardDeck.Cards));

        _startDealWhenСonditionTrueCoroutine = null;
    }

    private void SetStageCoroutine(GameStage gameStage)
    {
        switch (gameStage)
        {
            case GameStage.Preflop:
                _stageCoroutine = StartPreflop();
                break;
            
            case GameStage.Flop:
            case GameStage.Turn:
            case GameStage.River:
                _stageCoroutine = StartMidGameStage();
                break;
            
            case GameStage.Showdown:
                _stageCoroutine = StartShowdown();
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(_currentGameStage), _currentGameStage.Value, null);
        }
    }
    
    private static bool IsBetsEquals()
    {
        return PlayerSeats.Players.Where(x => x != null && x.BetAction != BetAction.Fold).Select(x => x.BetAmount).Distinct().Skip(1).Any() == false;
    }
    
    #region RPC

    [ServerRpc]
    private void ChangeIsStageCoroutineOverValueServerRpc(bool value)
    {
        _isStageCoroutineOver.Value = value;
    }

    [ServerRpc]
    private void SetIsPlayingValueServerRpc(bool value)
    {
        _isPlaying.Value = value;
    }

    [ServerRpc]
    private void SetCurrentGameStageValueServerRpc(GameStage value)
    {
        _currentGameStage.Value = value;
    }

    [ServerRpc]
    private void SetCodedBoardCardsValueServerRpc(string value)
    {
        _codedBoardCardsString.Value = value;
    }

    [ClientRpc]
    private void SetPlayersPocketCardsClientRpc(ulong playerId, CardObject card1, CardObject card2)
    {
        Player player = PlayerSeats.Players.FirstOrDefault(x => x != null && x.OwnerClientId == playerId);
        if (player == null)
        {
            return;
        }
        
        player.SetPocketCards(card1, card2);
    }

    [ClientRpc]
    private void StartDealClientRpc(int[] cardDeck)
    {
        _cardDeck = new CardDeck(cardDeck);
        _board = new Board(_cardDeck.PullCards(5).ToList());
        
        Log.WriteToFile("Start Deal.");
        
        if (IsServer == false)
        {
            return;
        }
        
        _boardButton.Move();
        
        SetCodedBoardCardsValueServerRpc(CardObjectConverter.GetCodedCardsString(_board.Cards));
        SetIsPlayingValueServerRpc(true);
        
        StartNextStageClientRpc();
    }

    [ClientRpc]
    private void EndDealClientRpc(WinnerInfo[] winnerInfo)
    {
        if (IsServer == true)
        {        
            SetCurrentGameStageValueServerRpc(GameStage.Empty);
            SetIsPlayingValueServerRpc(false);
            SetCodedBoardCardsValueServerRpc(string.Empty);
        }

        if (_stageCoroutine != null)
        {
            StopCoroutine(_stageCoroutine);
        }
        
        EndDealEvent?.Invoke(winnerInfo);
        
        Log.WriteToFile($"End deal. Winner id(`s): '{string.Join(", ", winnerInfo.Select(x => x.WinnerId))}'. Winner hand: {winnerInfo[0].Combination}");

        if (_startDealAfterRoundsInterval != null)
        {
            StopCoroutine(_startDealAfterRoundsInterval);
        }

        _startDealAfterRoundsInterval = StartDealAfterRoundsInterval();
        StartCoroutine(_startDealAfterRoundsInterval);
    }

    [ClientRpc]
    private void StartNextStageClientRpc()
    {
        GameStage nextStage = _currentGameStage.Value + 1;
        
        if (IsServer == true)
        {        
            ChangeIsStageCoroutineOverValueServerRpc(false);
            SetCurrentGameStageValueServerRpc(nextStage);
        }
        
        Log.WriteToFile($"Starting {nextStage} stage.");               
        GameStageBeganEvent?.Invoke(nextStage);

        SetStageCoroutine(nextStage);
        StartCoroutine(_stageCoroutine);
    }

    [ClientRpc]
    private void EndStageClientRpc()
    {
        GameStageOverEvent?.Invoke(GameStage.Showdown);
    }
    
    #endregion
}