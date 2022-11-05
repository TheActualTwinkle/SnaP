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

    private CardDeck _cardDeck;
    private Board _board;

    private bool _conditionToStartDeal => (_isPlaying == false) && (_playerSeats?.CountOfTakenSeats >= 2);

    private void OnValidate()
    {
        _bigBlindValue = _smallBlindValue * 2;
    }

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
        if (_conditionToStartDeal == true)
        {
            StartDeal();
        }
        else
        {
            StartCoroutine(StartDealWhenÑondition());
        }
    }

    private void OnPlayerLeave(Player player, int seatNumber)
    {
        if (_playerSeats.CountOfTakenSeats == 1)
        {
            EndDeal(_playerSeats.Players.Where(x => x != null).FirstOrDefault());
        }
    }

    private IEnumerator StartDealWhenÑondition()
    {
        yield return new WaitUntil(() => _conditionToStartDeal == true);
        StartDeal();
    }
}