using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Unity.Netcode;
using UnityEngine.UIElements;
using System.Globalization;

public class PlayerSeats : MonoBehaviour
{
    public static PlayerSeats Instance { get; private set; }

    public const int MaxSeats = 5;

    public event Action<Player, int> PlayerSitEvent;
    public event Action<Player, int> PlayerLeaveEvent;

    public List<Player> Players => _players.ToList();
    [ReadOnly]
    [SerializeField] private List<Player> _players;

    public int CountOfTakenSeats => _players.Where(x => x != null).Count();

    [SerializeField] private float _conncetionLostCheckInterval;

    private void OnValidate()
    {
        _players = new List<Player>(MaxSeats);
        for (int i = 0; i < MaxSeats; i++)
        {
            _players.Add(null);
        }
    }

    public void Awake()
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

    private void Start()
    {
        StartCoroutine(CheckForConnectonLost());
    }

    public bool TryTake(Player player, int seatNumber)
    {
        if (_players[seatNumber] != null)
        {
            Log.WriteLine($"Player ('{player.NickName}') can`t take the {seatNumber} seat, its already taken by Player('{_players[seatNumber].NickName}).'");
            return false;
        }

        TryLeave(player);

        _players[seatNumber] = player;

        Log.WriteLine($"Player ('{player.NickName}') sit on {seatNumber} seat.");

        PlayerSitEvent?.Invoke(player, seatNumber);
        return true;
    }

    public bool TryLeave(Player player)
    {
        if (_players.Contains(player) == false)
        {
            return false;
        }

        int seatNumber = _players.IndexOf(player);
        _players[seatNumber] = null;

        Log.WriteLine($"Player ('{player.NickName}') leave from {seatNumber} seat.");

        PlayerLeaveEvent?.Invoke(player, seatNumber);
        return true;
    }

    public bool IsFree(int seatNumber)
    {
        if (_players[seatNumber] == null)
        {
            return true;
        }

        return false;
    }

    private IEnumerator CheckForConnectonLost()
    {
        while (true)
        {
            Log.WriteLine($"CheckForConnectonLost cycle", $"{Application.persistentDataPath}\\CustomLog.log");

            for (int i = 0; i < _players.Count; i++)
            {
                try
                {
                    bool nullReferences = _players[i].HasNetworkObject;
                    Log.WriteLine($"Connection stable on '{_players[i].NickName}'", $"{Application.persistentDataPath}\\CustomLog.log");
                }
                catch (NullReferenceException)
                {
                    try
                    {
                        // Check for MissingReferenceException (Kolhoz becouse cant catch the real MissingReferenceException).
                        string nick = _players[i].NickName;
                        Log.WriteLine($"Conncetion lost on player ('{nick}') on {i} seat.", $"{Application.persistentDataPath}\\CustomLog.log");
                        TryLeave(_players[i]);
                    }
                    catch { }
                }
            }

            yield return new WaitForSeconds(_conncetionLostCheckInterval);
        }
    }
}
