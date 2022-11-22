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

    [ReadOnly] [SerializeField] private bool _isPlaying;

    public uint CallAmount => _callAmount;
    [ReadOnly] [SerializeField] private uint _callAmount;

    [ReadOnly] [SerializeField] private uint _pot;

    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    [ReadOnly] [SerializeField] private Board _board;
    [ReadOnly] [SerializeField] private BoardButton _boardButton;
    private CardDeck _cardDeck;
    
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
                StartPreflop();
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
        
        GameStageChangedEvent?.Invoke(gameStage);
    }

    private void StartPreflop()
    {
        _board = new Board(_cardDeck.PullCards(5).ToList());

        foreach (int index in _boardButton.TurnSequensce)
        {
            PlayerSeats.Players[index].SetPocketCards(_cardDeck.PullCard(), _cardDeck.PullCard());
        }
    }

    private void OnPlayerSit(Player player, int seatNumber)
    {
        if (IsServer == false)
        {
            return;
        }

        _cardDeck = new CardDeck();
        if (ConditionToStartDeal == true)
        {
            StartDealClientRpc(_cardDeck.GetCodedCards());
        }
        else
        {
            StartCoroutine(StartDealWhenСonditionTrue());
        }
    }

    private void OnPlayerLeave(Player player, int seatNumber)
    {
        if (IsServer == false)
        {
            return;
        }
        
        if (PlayerSeats.TakenSeatsAmount == 1)
        {
            ulong winnerId = PlayerSeats.Players.FirstOrDefault(x => x != null).OwnerClientId;
            WinnerData winnerData = new(winnerId, _pot);
            EndDealClientRpc(winnerData);
        }
    }

    private IEnumerator StartDealWhenСonditionTrue()
    {
        yield return new WaitUntil(() => ConditionToStartDeal == true);
        StartDealClientRpc(_cardDeck.GetCodedCards());
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

        // Not pot but some chips based on bet.
        EndDealEvent?.Invoke(winnerData);
    }
    
    #endregion
}