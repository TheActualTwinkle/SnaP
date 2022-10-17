using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class PlayerSeats : MonoBehaviour
{
    public static PlayerSeats Instance => _instance;
    private static PlayerSeats _instance;

    public event Action<Player, int> PlayerSitEvent;
    public event Action<Player, int> PlayerLeaveEvent;

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

    public bool TryTake(Player player, int seatNumber)
    {
        if (_players[seatNumber] != null)
        {
            Debug.LogError($"{player.NickName} can`t take the {seatNumber} seat, its already taken by '{Players[seatNumber].NickName}'");
            return false;
        }

        Debug.Log($"Player '{player.NickName}' sit on {seatNumber} seat.");

        _players[seatNumber] = player;
        PlayerSitEvent?.Invoke(player, seatNumber);

        return true;
    }

    public bool TryLeave(Player player)
    {
        if (_players.Contains(player) == false)
        {
            Debug.Log($"{player.NickName} not found. He cant leave");
            return false;
        }

        int seatNumber = _players.IndexOf(player);
        _players[seatNumber] = null;

        PlayerLeaveEvent?.Invoke(player, seatNumber);

        return true;
    }
}
