using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    public static Game Instance { get; private set; }

    public event Action<GameStage> GameStageBeganEvent;
    public event Action<GameStage> GameStageOverEvent;
    public event Action<WinnerInfo> EndDealEvent;

    public List<CardObject> BoardCards => _board.Cards;
    
    public bool IsPlaying => _isPlaying;
    [ReadOnly] [SerializeField] private bool _isPlaying;

    private static Betting Betting => Betting.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    [ReadOnly] [SerializeField] private Board _board;
    [ReadOnly] [SerializeField] private BoardButton _boardButton;
    private CardDeck _cardDeck;

    private IEnumerator _stageCoroutine;
    private IEnumerator _startDealWhenСonditionTrueCoroutine;

    private GameStage _currentGameStage = (GameStage)(-1);
    
    private bool ConditionToStartDeal => (_isPlaying == false) && (PlayerSeats.TakenSeatsAmount >= 2);

    [SerializeField] private float _roundsInterval;
    
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

    private void StartNextStage()
    {
        _currentGameStage++; 
        
        Log.WriteLine($"Starting {_currentGameStage} stage.");
        
        switch (_currentGameStage)
        {
            case GameStage.Preflop:
                _stageCoroutine = StartPreflop();
                break;
            
            case GameStage.Flop:
            case GameStage.Turn:
            case GameStage.River:
                _stageCoroutine = StartMidgameStage();
                break;
            
            case GameStage.Showdown:
                throw new NotImplementedException("Showdown где?!");
            default:
                throw new ArgumentOutOfRangeException(nameof(_currentGameStage), _currentGameStage, null);
        }
        
        StartCoroutine(_stageCoroutine);
        GameStageBeganEvent?.Invoke(_currentGameStage);
    }

    private IEnumerator StartPreflop()
    {
        _board = new Board(_cardDeck.PullCards(5).ToList());

        int[] turnSequensce = _boardButton.GetTurnSequensce();
        foreach (int index in turnSequensce)
        {
            PlayerSeats.Players[index].SetPocketCards(_cardDeck.PullCard(), _cardDeck.PullCard());
        }
        
        AutoBetBlinds();

        int[] preflopTurnSequensce = _boardButton.GetPreflopTurnSequensce();
        yield return Bet(preflopTurnSequensce);
        
        GameStageOverEvent?.Invoke(GameStage.Preflop);
        
        yield return new WaitForSeconds(_roundsInterval);

        StartNextStage();
    }

    // Stage like Flop, Turn and River
    private IEnumerator StartMidgameStage()
    {
        int[] turnSequensce = _boardButton.GetTurnSequensce();
        yield return Bet(turnSequensce);
        
        GameStageOverEvent?.Invoke(_currentGameStage);

        yield return new WaitForSeconds(_roundsInterval);
        StartNextStage();
    }

    private IEnumerator StartShowdown()
    {
        throw new NotImplementedException("StartShowdown func.  ");
    }

    private void AutoBetBlinds()
    {
        int[] turnSequensce = _boardButton.GetTurnSequensce();
        PlayerSeats.Players[turnSequensce[0]].TryBet(Betting.SmallBlind);
        PlayerSeats.Players[turnSequensce[1]].TryBet(Betting.BigBlind);
    }
    
    private IEnumerator Bet(IEnumerable<int> turnSequensce)
    {
        foreach (int index in turnSequensce)
        {
            Player player = PlayerSeats.Players[index];

            if (player == null)
            {
                continue;
            }

            int foldPlayerAmount = PlayerSeats.Players.Count(x => x != null && x.ChoosenBetAction == BetAction.Fold);
            
            if (PlayerSeats.TakenSeatsAmount - foldPlayerAmount == 1)
            {
                ulong winnerId = player.OwnerClientId;
                WinnerInfo winnerInfo = new(winnerId, Pot.Instance.GetWinValue(player));
                EndDealClientRpc(winnerInfo);

                StartCoroutine(StartDealAfterIntervalRounds());
                yield break;
            }

            Log.WriteLine($"Start seat №{index}");
            yield return StartCoroutine(Betting.Bet(player));
            Log.WriteLine($"End seat №{index}");
        }
    }
        
    private void OnPlayerSit(Player player, int seatNumber)
    {
        if (IsServer == false)
        {
            return;
        }
        
        if (_startDealWhenСonditionTrueCoroutine != null)   
        {
            return;
        }
        
        _startDealWhenСonditionTrueCoroutine = StartDealWhenСonditionTrue();
        StartCoroutine(_startDealWhenСonditionTrueCoroutine);
    }

    private void OnPlayerLeave(Player player, int seatNumber)
    {
        if (IsServer == false)
        {
            return;
        }
        
        if (PlayerSeats.TakenSeatsAmount != 1)
        {
            return;
        }

        Player winner = PlayerSeats.Players.FirstOrDefault(x => x != null);
        ulong winnerId = winner!.OwnerClientId; 
        WinnerInfo winnerInfo = new(winnerId, Pot.Instance.GetWinValue(winner));
        EndDealClientRpc(winnerInfo);
    }

    private IEnumerator StartDealAfterIntervalRounds()
    {
        yield return new WaitForSeconds(_roundsInterval);
        StartCoroutine(StartDealWhenСonditionTrue());
    }
    
    private IEnumerator StartDealWhenСonditionTrue()
    {
        yield return new WaitUntil(() => ConditionToStartDeal == true);
        yield return new WaitForSeconds(0.05f);

        _cardDeck = new CardDeck();
        StartDealClientRpc(_cardDeck.GetCodedCards());

        _startDealWhenСonditionTrueCoroutine = null;
    }

    #region RPC

    [ClientRpc]
    private void StartDealClientRpc(int[] cardDeck)
    {
        _isPlaying = true;
        _cardDeck = new CardDeck(cardDeck);

        _boardButton ??= new BoardButton();
        _boardButton.Move();
        
        _currentGameStage = (GameStage)(-1);
        
        StartNextStage();
    }

    [ClientRpc]
    private void EndDealClientRpc(WinnerInfo winnerInfo)
    {
        _isPlaying = false;

        if (_stageCoroutine != null)
        {
            StopCoroutine(_stageCoroutine);
        }
        
        PlayerSeats.SitEveryoneWaiting();

        // Not pot but some chips based on bet.
        EndDealEvent?.Invoke(winnerInfo);
        Log.WriteLine($"End deal. Winner id: {winnerInfo.WinnerId}");
    }
    
    #endregion
}