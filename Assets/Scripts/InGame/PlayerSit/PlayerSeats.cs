using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerSeats : MonoBehaviour
{
    public event Action<PlayerSeatData> PlayerSitEvent;
    public event Action<PlayerSeatData> PlayerLeaveEvent;

    [SerializeField] private PlayerSeatUI _playerSeatUI;

    public List<Player> Players => _players.ToList();
    [ReadOnly]
    [SerializeField] private List<Player> _players;

    public const int MAX_SEATS = 5;

    public int CountOfFreeSeats => Players.Where(x => x == null).Count();

    private void OnValidate()
    {
        _players?.Clear();
        for (int i = 0; i < MAX_SEATS; i++)
        {
            _players.Add(null);
        }
    }

    private void OnEnable()
    {
        _playerSeatUI.PlayerClickJoinButton += OnPlayerClickJoinButton;
    }

    private void OnDisable()
    {
        _playerSeatUI.PlayerClickJoinButton -= OnPlayerClickJoinButton;
    }

    private bool TryTake(PlayerSeatData playerSeatData)
    {
        if (CountOfFreeSeats == 0)
        {
            Debug.LogError($"There is no free seats here");
            return false;
        }

        if (Players[playerSeatData.SeatNumber] != null)
        {
            Debug.LogError($"{playerSeatData.Player.NickName} can`t take the {playerSeatData.SeatNumber} seat, its already taken");
            return false;
        }

        _players[playerSeatData.SeatNumber] = playerSeatData.Player;
        PlayerSitEvent?.Invoke(playerSeatData);
        return true;
    }

    private void Leave(PlayerSeatData playerSeatData)
    {
        _players[playerSeatData.SeatNumber] = null;
        PlayerLeaveEvent?.Invoke(playerSeatData);
    }

    private void OnPlayerClickJoinButton(PlayerSeatData data)
    {
        TryTake(data);
    }
}
