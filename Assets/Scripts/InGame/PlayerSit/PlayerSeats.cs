using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerSeats : NetworkBehaviour
{
    public static PlayerSeats Instance => _instance;
    private static PlayerSeats _instance;

    public event Action<Player> PlayerSitEvent;
    public event Action<Player> PlayerLeaveEvent;

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

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool TryTake(Player player)
    {
        int seatNumber = player.SeatNumber.Value;

        if (_players[seatNumber] != null)
        {
            Debug.LogError($"{player.NickName} can`t take the {seatNumber} seat, its already taken by {Players[seatNumber].NickName}");
            return false;
        }

        Debug.Log($"Player '{player.NickName}' sit on {seatNumber} seat. Is owner: {IsOwner}");

        _players[seatNumber] = player;
        PlayerSitEvent?.Invoke(player);

        return true;
    }

    public void Leave(Player player)
    {
        int seatNumber = player.SeatNumber.Value;
        _players[seatNumber] = null;

        PlayerLeaveEvent?.Invoke(player);
    }
}
