using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerSeats : MonoBehaviour
{
    public event Action PlayerSitEvent;
    public event Action PlayerLeaveEvent;
    
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

    public bool TryTake(Player player, int seatNumber)
    {
        if (CountOfFreeSeats == 0)
        {
            Debug.LogError($"There is no free seats here");
            return false;
        }

        if (Players[seatNumber] != null)
        {
            Debug.LogError($"{player.NickName} can`t take the {seatNumber} seat, its already taken");
            return false;
        }

        _players[seatNumber] = player;
        PlayerSitEvent?.Invoke();
        return true;
    }

    public void Leave(int seatNumber)
    {
        _players[seatNumber] = null;
        PlayerLeaveEvent?.Invoke();
    }
}
