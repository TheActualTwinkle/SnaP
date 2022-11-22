using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<WinnerData> EndDealEvent;
    public event Action<GameStage> GameStageChangedEvent;

    public bool IsPlaying => _isPlaying;
    [ReadOnly]
    [SerializeField] private bool _isPlaying;

    public uint CallAmount => _callAmount;
    [ReadOnly]
    [SerializeField] private uint _callAmount;

    [ReadOnly]
    [SerializeField] private uint _pot;

    [ReadOnly]
    [SerializeField] private uint _bigBlindValue;
    [SerializeField] private uint _smallBlindValue;

    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    private CardDeck _cardDeck;
    private Board _board;

    private bool ConditionToStartDeal => (_isPlaying == false) && (PlayerSeats.CountOfTakenSeats >= 2);

    private void OnValidate()
    {
        _bigBlindValue = _smallBlindValue * 2;
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

    private void StartDeal()
    {
        _isPlaying = true;

        _cardDeck = new CardDeck();
        _cardDeck.Initialize();

        var boradCards = new List<CardObject>();
        for (var i = 0; i < 5; i++)
        {
            boradCards.Add(_cardDeck.PullCard());
        }
        _board = new Board(boradCards);

        StartCoroutine(StartNextStage(GameStage.Preflop));
    }

    private IEnumerator StartNextStage(GameStage gameStage)
    {
        GameStageChangedEvent?.Invoke(gameStage);

        yield return null;
    }

    private void EndDeal(Player winner)
    {
        _isPlaying = false;

        // Not pot but some chips based on bet.
        EndDealEvent?.Invoke(new WinnerData(winner, _pot));
    }

    private void OnPlayerSit(Player player, int seatNumber)
    {
        if (ConditionToStartDeal == true)
        {
            StartDeal();
        }
        else
        {
            StartCoroutine(StartDealWhenСondition());
        }
    }

    private void OnPlayerLeave(Player player, int seatNumber)
    {
        if (PlayerSeats.CountOfTakenSeats == 1)
        {
            EndDeal(PlayerSeats.Players.FirstOrDefault(x => x != null));
        }
    }

    private IEnumerator StartDealWhenСondition()
    {
        yield return new WaitUntil(() => ConditionToStartDeal == true);
        StartDeal();
    }
}