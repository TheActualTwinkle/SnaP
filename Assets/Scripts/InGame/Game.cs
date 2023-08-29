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
            Log.WriteToFile("Error: Preflop stage wanted to performed on client.");
            yield break;
        }

        int[] turnSequence = _boardButton.GetTurnSequence();
        
        Player player1 = PlayerSeats.Players[turnSequence[0]];
        Player player2 = PlayerSeats.Players[turnSequence[1]];
        yield return Betting.AutoBetBlinds(player1, player2);

        int[] preflopTurnSequence = _boardButton.GetPreflopTurnSequence();

        yield return Bet(preflopTurnSequence);
        
        S_EndStage();

        yield return new WaitForSeconds(_roundsInterval);

        S_StartNextStage();
    }

    // Stage like Flop, Turn and River.
    private IEnumerator StartMidGameStage()
    {
        if (IsServer == false)
        {
            Log.WriteToFile("Error: MidGame stage wanted to be performed on client.");
            yield break;
        }
        
        if (Betting.IsAllIn == false)
        {
            int[] turnSequence = _boardButton.GetTurnSequence();
            yield return Bet(turnSequence);
        }

        S_EndStage();

        yield return new WaitForSeconds(_roundsInterval);
        
        S_StartNextStage();
    }

    private IEnumerator StartShowdown()
    {
        if (IsServer == false)
        {
            Log.WriteToFile("Error: Showdown stage wanted to performed on client.");
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

        S_EndStage();
        
        yield return new WaitForSeconds(_showdownEndTime);

        List<WinnerInfo> winnerInfo = new();
        foreach (Player winner in winners)
        {
            winnerInfo.Add(new WinnerInfo(winner.OwnerClientId, Pot.GetWinValue(winner, winners), winnerHand.ToString()));
        }

        S_EndDeal(winnerInfo.ToArray());
    }

    private IEnumerator Bet(int[] turnSequence)
    {
        if (IsServer == false)
        {
            Log.WriteToFile("Error: Betting wanted to performed on client.");
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
                    WinnerInfo[] winnerInfo = {new(winnerId, Pot.GetWinValue(notFoldPlayers[0], new []{notFoldPlayers[0]}), "opponent(`s) folded")};
                    S_EndDeal(winnerInfo);
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
        WinnerInfo[] winnerInfo = {new(winnerId, Pot.GetWinValue(winner, new []{winner}), "opponent(`s) folded.")};
        S_EndDeal(winnerInfo);
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

        S_StartDeal();

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
    
    #region Server

    private void S_StartDeal()
    {
        if (IsServer == false)
        {
            return;
        }
        
        Log.WriteToFile("Starting Deal.");
        
        _cardDeck = new CardDeck();

        StartDealClientRpc(CardObjectConverter.GetCodedCards(_cardDeck.Cards));
        
        if (IsHost == false)
        {
            InitDeck(CardObjectConverter.GetCodedCards(_cardDeck.Cards));
            InitPocketCards();
            InitBoard();
        }

        _boardButton.Move();

        _codedBoardCardsString.Value = CardObjectConverter.GetCodedCardsString(_board.Cards);
        _isPlaying.Value = true;

        S_StartNextStage();
    }
    
    private void S_EndDeal(WinnerInfo[] winnerInfo)
    {
        if (IsServer == false)
        {
            return;
        }

        _currentGameStage.Value = GameStage.Empty;
        _isPlaying.Value = false;
        _codedBoardCardsString.Value = string.Empty;
        
        List<Player> winners = PlayerSeats.Players.Where(x => x != null && winnerInfo.Select(info => info.WinnerId).Contains(x.OwnerClientId)).ToList();

        Log.WriteToFile($"End deal. Pot {winnerInfo[0].Chips}. Winner(`s): ({string.Join(", ", winners)}). Winner hand: {winnerInfo[0].Combination}");

        if (_stageCoroutine != null)
        {
            StopCoroutine(_stageCoroutine);
        }
        
        if (IsHost == false)
        {
            InitEndDealRoutine(winnerInfo);
        }
        
        EndDealClientRpc(winnerInfo);
    }
    
    private void S_StartNextStage()
    {
        if (IsServer == false)
        {
            return;
        }

        GameStage stage = _currentGameStage.Value + 1;

        _currentGameStage.Value = stage;
        SetStageCoroutine(stage);
        StartCoroutine(_stageCoroutine);
        
        if (IsHost == false)
        {
            InvokeGameStageBeganEvent(stage);
        }
        
        StartNextStageClientRpc(stage);
        
        Log.WriteToFile($"Starting {stage} stage.");
    }

    private void S_EndStage()
    {
        if (IsServer == false)
        {
            return;
        }

        if (IsHost == false)
        {
            InvokeGameStageOverEvent(_currentGameStage.Value);
        }
        
        EndStageClientRpc(_currentGameStage.Value);
    }
    
    #endregion
    
    #region RPC

    [ClientRpc]
    private void SetPlayersPocketCardsClientRpc(ulong playerId, CardObject card1, CardObject card2)
    {
        SetPlayersPocketCards(playerId, card1, card2);
    }

    [ClientRpc]
    private void StartDealClientRpc(int[] cardDeck)
    {
        InitDeck(cardDeck);
        InitPocketCards();
        InitBoard();
    }

    [ClientRpc]
    private void EndDealClientRpc(WinnerInfo[] winnerInfo)
    {
        InitEndDealRoutine(winnerInfo);
    }

    [ClientRpc]
    private void StartNextStageClientRpc(GameStage stage)
    {
        InvokeGameStageBeganEvent(stage);
    }

    [ClientRpc]
    private void EndStageClientRpc(GameStage stage)
    {
        InvokeGameStageOverEvent(stage);
    }
    
    #endregion

    #region Methods that has to be called both on Server and Client.

    private void SetPlayersPocketCards(ulong playerId, CardObject card1, CardObject card2)
    {
        Player player = PlayerSeats.Players.FirstOrDefault(x => x != null && x.OwnerClientId == playerId);
        if (player == null)
        {
            return;
        }
        
        player.SetPocketCards(card1, card2);
    }
    
    private void InitDeck(int[] cardDeck)
    {
        _cardDeck = new CardDeck(cardDeck);

        if (IsServer == true)
        {
            Log.WriteToFile($"Deck created: {string.Join(", ", cardDeck)}.");
        }
    }


    private void InitPocketCards()
    {
        int[] turnSequence = _boardButton.GetTurnSequence();
        foreach (int index in turnSequence)
        {
            Player player = PlayerSeats.Players[index];
            CardObject card1 =  _cardDeck.PullCard();
            CardObject card2 =  _cardDeck.PullCard();
            
            if (IsHost == false)
            {
                SetPlayersPocketCards(player.OwnerClientId, card1, card2);
            }
            
            SetPlayersPocketCardsClientRpc(player.OwnerClientId, card1, card2);

            if (IsServer == true)
            {            
                Log.WriteToFile($"Player ('{player}') received: {card1}, {card2}.");
            }
        }
    }

    private void InitBoard()
    {
        _board = new Board(_cardDeck.PullCards(5).ToList());

        if (IsServer == true)
        {
            Log.WriteToFile($"Board created: {string.Join(", ", _board.Cards)}.");
        }
    }
        
    private void InitEndDealRoutine(WinnerInfo[] winnerInfo)
    {
        if (_startDealAfterRoundsInterval != null)
        {
            StopCoroutine(_startDealAfterRoundsInterval);
        }

        _startDealAfterRoundsInterval = StartDealAfterRoundsInterval();
        StartCoroutine(_startDealAfterRoundsInterval);
        
        EndDealEvent?.Invoke(winnerInfo);
    }
    
    private void InvokeGameStageBeganEvent(GameStage stage)
    {
        GameStageBeganEvent?.Invoke(stage);
    }

    private void InvokeGameStageOverEvent(GameStage stage)
    {
        GameStageOverEvent?.Invoke(stage);
    }

    #endregion
}