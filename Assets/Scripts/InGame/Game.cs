using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    public static Game Instance { get; private set; }

    public event Action<GameStage> GameStageChangedEvent;
    public event Action<WinnerData> EndDealEvent;

    public bool IsPlaying => _isPlaying;
    [ReadOnly] [SerializeField] private bool _isPlaying;

    [ReadOnly] [SerializeField] private uint _pot;

    private static Betting Betting => Betting.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    [ReadOnly] [SerializeField] private Board _board;
    [ReadOnly] [SerializeField] private BoardButton _boardButton;
    private CardDeck _cardDeck;

    private IEnumerator _stageCoroutine;
    private IEnumerator _startDealWhenСonditionTrueCoroutine;
    
    private bool ConditionToStartDeal => (_isPlaying == false) && (PlayerSeats.TakenSeatsAmount >= 2);

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

    private void StartNextStage(GameStage gameStage)
    {
        switch (gameStage)
        {
            case GameStage.Preflop:
                _stageCoroutine = StartPreflop();
                break;
            case GameStage.Flop:
                break;
            case GameStage.Turn:
                break;
            case GameStage.River:
                break;
            case GameStage.Showdown:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameStage), gameStage, null);
        }
        
        StartCoroutine(_stageCoroutine);
        GameStageChangedEvent?.Invoke(gameStage);
    }

    private IEnumerator StartPreflop()
    {
        _board = new Board(_cardDeck.PullCards(5).ToList());

        int[] turnSequensce = _boardButton.GetTurnSequensce();
        foreach (int index in turnSequensce)
        {
            PlayerSeats.Players[index].SetPocketCards(_cardDeck.PullCard(), _cardDeck.PullCard());
        }
        
        PlayerSeats.Players[turnSequensce[0]].TryBet(Betting.SmallBlind);
        PlayerSeats.Players[turnSequensce[1]].TryBet(Betting.BigBlind);

        int[] preflopTurnSequensce = _boardButton.GetPreflopTurnSequensce();
        foreach (int index in preflopTurnSequensce)
        {
            Player player = PlayerSeats.Players[index];

            if (player == null)
            {
                continue;
            }
            
            Log.WriteLine($"Ходит сиденье №{index}");
            yield return StartCoroutine(Betting.Bet(player));
            Log.WriteLine($"Завершил ход сиденье №{index}");
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
        
        _cardDeck = new CardDeck();
        
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
        
        ulong winnerId = PlayerSeats.Players.FirstOrDefault(x => x != null)!.OwnerClientId;
        WinnerData winnerData = new(winnerId, _pot);
        EndDealClientRpc(winnerData);
    }

    private IEnumerator StartDealWhenСonditionTrue()
    {
        yield return new WaitUntil(() => ConditionToStartDeal == true);
        yield return new WaitForSeconds(0.05f);
        StartDealClientRpc(_cardDeck.GetCodedCards());

        _startDealWhenСonditionTrueCoroutine = null;
    }

    
    #region RPC

    [ClientRpc]
    private void StartDealClientRpc(int[] cardDeck)
    {
        _isPlaying = true;
        
        _cardDeck = new CardDeck(cardDeck);
        _boardButton = new BoardButton();
        _boardButton.Move();
        
        StartNextStage(GameStage.Preflop);
    }

    [ClientRpc]
    private void EndDealClientRpc(WinnerData winnerData)
    {
        _isPlaying = false;
        StopCoroutine(_stageCoroutine);        
        
        PlayerSeats.SitEveryoneWaiting();

        // Not pot but some chips based on bet.
        EndDealEvent?.Invoke(winnerData);
        Log.WriteLine($"End deal. Winner id: {winnerData.WinnerId}");
    }
    
    #endregion
}