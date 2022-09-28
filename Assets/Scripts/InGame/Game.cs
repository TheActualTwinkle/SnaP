using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    public event Action<Player> PlayerTurnBegunEvent;
    public event Action<WinnerData> EndDealEvent;
    public event Action<GameStages> GameStageChangedEvent;

    public bool IsPlaying => _isPlaying;
    [ReadOnly]
    [SerializeField] private bool _isPlaying;

    [ReadOnly]
    [SerializeField] private uint _pot;

    [ReadOnly]
    [SerializeField] private uint _bigBlindValue;

    [SerializeField] private uint _smallBlindValue;

    [SerializeField] private PlayerSeats _playerSeats;
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
    }

    private void StartPreflop()
    {
        GameStageChangedEvent?.Invoke(GameStages.Preflop);

        StartCoroutine(MakeBets());
    }

    private void StartFlop()
    {
        GameStageChangedEvent?.Invoke(GameStages.Flop);
    }

    private void StartTrun()
    {
        GameStageChangedEvent?.Invoke(GameStages.Turn);
    }

    private void StartRiver()
    {
        GameStageChangedEvent?.Invoke(GameStages.River);
    }

    private void StartShowdown()
    {
        GameStageChangedEvent?.Invoke(GameStages.Showdown);
    }

    private void EndDeal(Player winner)
    {
        _isPlaying = false;

        WinnerData winnerData = new WinnerData(winner, _pot);
        EndDealEvent?.Invoke(winnerData);
    }

    private void OnPlayerSit()
    {
        if (_isPlaying == false && _playerSeats?.CountOfFreeSeats >= 2)
        {
            StartDeal();
        }
    }

    private void OnPlayerLeave()
    {
        if (_playerSeats?.CountOfFreeSeats == PlayerSeats.MAX_SEATS - 1)
        {
            EndDeal(_playerSeats.Players.Where(x => x != null).FirstOrDefault());
        }
    }

    private IEnumerator MakeBets()
    {
        foreach (var player in _playerSeats.Players)
        {
            if (player != null)
            {
                PlayerTurnBegunEvent?.Invoke(player);
            }
        }
        yield return null;
    }
}
