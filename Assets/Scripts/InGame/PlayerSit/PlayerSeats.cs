using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerSeats : NetworkBehaviour
{
    public event Action<PlayerSeatData> PlayerSitEvent;
    public event Action<PlayerSeatData> PlayerLeaveEvent;

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

    public bool TryTake(PlayerSeatData data)
    {
        if (Players[data.SeatNumber] != null)
        {
            Debug.LogError($"{data.Player.NickName} can`t take the {data.SeatNumber} seat, its already taken by {Players[data.SeatNumber].NickName}");
            return false;
        }

        if (_players.Contains(data.Player))
        {
            int oldSeatNumber = _players.IndexOf(data.Player);
            Leave(new PlayerSeatData(data.Player, oldSeatNumber));
        }

        _players[data.SeatNumber] = data.Player;
        PlayerSitEvent?.Invoke(data);
        return true;
    }

    public void Leave(PlayerSeatData data)
    {
        _players[data.SeatNumber] = null;
        PlayerLeaveEvent?.Invoke(data);
    }
}
