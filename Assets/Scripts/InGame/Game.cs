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

    public GameStage CurrentGameStage => _currentGameStage;
    private GameStage _currentGameStage = (GameStage)(-1);
    
    private bool ConditionToStartDeal => _isPlaying == false && PlayerSeats.TakenSeatsAmount >= 2;

    [SerializeField] private float _roundsInterval;

    // This field is for CLIENTS. It`s tracking when Server/Host calls the 'EndBetCoroutineClientRpc' so when it`s called sets true and routine ends. 
    [ReadOnly] [SerializeField] private bool _isBetCoroutineOver;

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
        _board = new Board(_cardDeck.PullCards(5).ToList());

        int[] turnSequensce = _boardButton.GetTurnSequence();
        foreach (int index in turnSequensce)
        {
            PlayerSeats.Players[index].SetPocketCards(_cardDeck.PullCard(), _cardDeck.PullCard());
        }
        
        AutoBetBlinds();

        int[] preflopTurnSequensce = _boardButton.GetPreflopTurnSequence();

        yield return Bet(preflopTurnSequensce);

        GameStageOverEvent?.Invoke(GameStage.Preflop);
        
        yield return new WaitForSeconds(_roundsInterval);

        StartNextStageClientRpc();
    }

    // Stage like Flop, Turn and River
    private IEnumerator StartMidgameStage()
    {
        int[] turnSequensce = _boardButton.GetTurnSequence();
        yield return Bet(turnSequensce);
        
        GameStageOverEvent?.Invoke(_currentGameStage);

        yield return new WaitForSeconds(_roundsInterval);
        
        StartNextStageClientRpc();
    }

    private IEnumerator StartShowdown() // todo MB not IEnumerator?
    {
        int[] turnSequensce = _boardButton.GetShowdownTurnSequence();
        
        Player winner = null;
        Hand winnerHand = new();
        for (var i = 0; i < turnSequensce.Length; i++)
        {
            Player player = PlayerSeats.Players[turnSequensce[i]];
            List<CardObject> completeCards = _board.Cards.ToList();
            completeCards.Add(player.PocketCard1); completeCards.Add(player.PocketCard2);

            Hand bestHand = CombinationСalculator.GetBestHand(new Hand(completeCards));

            if (i == 0 || bestHand > winnerHand)
            {
                winner = player;
                winnerHand = bestHand;
            }
            else if (bestHand == winnerHand)
            {
                print("TIE!");
                print(bestHand + " vs " + winnerHand);
                yield break;
                // todo Force Tie.
            }
        }

        if (winner == null)
        {
            throw new NullReferenceException();
        }
        
        yield return new WaitForSeconds(3f); // todo Hardcode.
        
        WinnerInfo winnerInfo = new(winner.OwnerClientId, Pot.Instance.GetWinValue(winner));
        EndDealClientRpc(winnerInfo);
        print(winnerHand);
    }

    private void AutoBetBlinds()
    {
        int[] turnSequensce = _boardButton.GetTurnSequence();
        PlayerSeats.Players[turnSequensce[0]].TryBet(Betting.SmallBlind);
        PlayerSeats.Players[turnSequensce[1]].TryBet(Betting.BigBlind);
    }
    
    private IEnumerator Bet(int[] turnSequensce)
    {
        _isBetCoroutineOver = false;
        if (IsServer == false)
        {
            yield return new WaitWhile(() => _isBetCoroutineOver == false);
            yield break;
        }

        for (var i = 0;; i++)
        {
            foreach (int index in turnSequensce)
            {
                Player player = PlayerSeats.Players[index];

                if (player == null)
                {
                    continue;
                }

                Log.WriteToFile($"Player ('{player.NickName}'). Seat №{index} start betting");
                yield return StartCoroutine(Betting.Bet(player));
            
                int foldPlayerAmount = PlayerSeats.Players.Count(x => x != null && x.BetAction == BetAction.Fold);
                
                if (PlayerSeats.TakenSeatsAmount - foldPlayerAmount == 1)
                {
                    ulong winnerId = player.OwnerClientId;
                    WinnerInfo winnerInfo = new(winnerId, Pot.Instance.GetWinValue(player));
                    EndDealClientRpc(winnerInfo);
                    yield break;
                }

                if (i == 0 || IsBetsEquals() == false)
                {
                    continue;
                }

                EndBetCoroutineClientRpc();
                yield break;
            }

            if (i != 0 || IsBetsEquals() == false)
            {
                continue;
            }

            EndBetCoroutineClientRpc();
            yield break;
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
        if (IsServer == false)
        {
            yield break;
        }
        
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

    private void SetStageCoroutine()
    {
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
                _stageCoroutine = StartShowdown();
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(_currentGameStage), _currentGameStage, null);
        }
    }
    
    private static bool IsBetsEquals()
    {
        return PlayerSeats.Players.Where(x => x != null).Select(x => x.BetAmount).Distinct().Skip(1).Any() == false;
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
        
        StartNextStageClientRpc();
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
        Log.WriteToFile($"End deal. Winner id: {winnerInfo.WinnerId}");

        StartCoroutine(StartDealAfterIntervalRounds());
    }

    [ClientRpc]
    private void StartNextStageClientRpc()
    {        
        _currentGameStage++;
        
        Log.WriteToFile($"Starting {_currentGameStage} stage.");

        SetStageCoroutine();
        
        StartCoroutine(_stageCoroutine);
        GameStageBeganEvent?.Invoke(_currentGameStage);
    }

    [ClientRpc]
    private void EndBetCoroutineClientRpc()
    {
        _isBetCoroutineOver = true;
    }
    
    #endregion
}