using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    public event Action<Player> PlayerTurnBegunEvent;
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

    private PlayerSeats _playerSeats => PlayerSeats.Instance;
    private PlayerBetUI _playerBetUI => PlayerBetUI.Instance;

    private CardDeck _cardDeck;
    private Board _board;

    private void OnEnable()
    {
        _playerSeats.PlayerSitEvent += OnPlayerSit;
        _playerSeats.PlayerLeaveEvent += OnPlayerLeave;
    }

    private void OnDisable()
    {
        _playerSeats.PlayerSitEvent -= OnPlayerSit;
        _playerSeats.PlayerLeaveEvent -= OnPlayerLeave;
    }

    private void OnValidate()
    {
        _bigBlindValue = _smallBlindValue * 2;
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

        List<CardObject> boradCards = new List<CardObject>();
        for (int i = 0; i < 5; i++)
        {
            boradCards.Add(_cardDeck.PullCard());
        }
        _board = new Board(boradCards);

        StartPreflop();
    }

    private void StartPreflop()
    {
        GameStageChangedEvent?.Invoke(GameStage.Preflop);

        StartCoroutine(MakeBetActions());
    }

    private void StartFlop()
    {
        GameStageChangedEvent?.Invoke(GameStage.Flop);
    }

    private void StartTrun()
    {
        GameStageChangedEvent?.Invoke(GameStage.Turn);
    }

    private void StartRiver()
    {
        GameStageChangedEvent?.Invoke(GameStage.River);
    }

    private void StartShowdown()
    {
        GameStageChangedEvent?.Invoke(GameStage.Showdown);
    }

    private void EndDeal(Player winner)
    {
        _isPlaying = false;

        // Not pot but some chips based on bet.
        EndDealEvent?.Invoke(new WinnerData(winner, _pot));
    }

    private void OnPlayerSit(Player player, int seatNumber)
    {
        if (_isPlaying == false && _playerSeats?.CountOfFreeSeats <= 3)
        {
            StartDeal();
        }
        else if (_isPlaying == true)
        {
            StartCoroutine(StartDealWhenPreviousOver());
        }
    }

    private void OnPlayerLeave(Player player, int seatNumber)
    {
        if (_playerSeats?.CountOfFreeSeats == PlayerSeats.MAX_SEATS - 1)
        {
            EndDeal(_playerSeats.Players.Where(x => x != null).FirstOrDefault());
        }
    }

    private IEnumerator MakeBetActions()
    {
        foreach (var player in _playerSeats.Players)
        {
            if (player != null)
            {
                PlayerTurnBegunEvent?.Invoke(player);
                yield return _playerBetUI.C_WaitForPlayerBet;
            }
        }
    }

    private IEnumerator StartDealWhenPreviousOver()
    {
        yield return new WaitUntil(() => (_isPlaying == false && _playerSeats?.CountOfFreeSeats >= 2));
        StartDeal();
    }
}