using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class PlayerSeats
{
    public const int MaxSeats = 5;
   
    public event Action<Player, int> PlayerSitEvent;
    public event Action<Player, int> PlayerLeaveEvent;

    public List<Player> Players => _players.ToList();
    [ReadOnly]
    [SerializeField] private List<Player> _players;

    public int CountOfFreeSeats => _players.Where(x => x == null).Count();

    public PlayerSeats()
    {
        _players = new List<Player>(MaxSeats);
        for (int i = 0; i < MaxSeats; i++)
        {
            _players.Add(null);
        }
    }

    public void Take(Player player, int seatNumber)
    {
        if (_players[seatNumber] != null)
        {
            Debug.LogError($"{player.NickName} can`t take the {seatNumber} seat, its already taken by '{_players[seatNumber].NickName}'");
            return;
        }

        if (_players.Contains(player) == true)
        {
            Leave(player);
        }

        _players[seatNumber] = player;
        Debug.Log($"Player '{player.NickName}' sit on {seatNumber} seat.");

        PlayerSitEvent?.Invoke(player, seatNumber);
    }

    public void Leave(Player player)
    {
        if (_players.Contains(player) == false)
        {
            Debug.LogError($"{player.NickName} not found. He cant leave");
            return;
        }

        int seatNumber = _players.IndexOf(player);
        _players[seatNumber] = null;

        PlayerLeaveEvent?.Invoke(player, seatNumber);
    }

    public bool IsFree(int seatNumber)
    {
        if (_players[seatNumber] == null)
        {
            return true;
        }

        return false;
    }
}
