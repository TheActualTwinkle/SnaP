using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSeats : MonoBehaviour
{
    public static PlayerSeats Instance { get; private set; }

    public const int MaxSeats = 5;

    public event Action<Player, int> PlayerSitEvent;
    public event Action<Player, int> PlayerWaitForSitEvent;
    public event Action<Player, int> PlayerLeaveEvent;
    
    public List<Player> Players => _players.ToList();
    [ReadOnly] [SerializeField] private List<Player> _players;

    public List<Player> WaitingPlayers => _waitingPlayers.ToList();
    [ReadOnly] [SerializeField] private List<Player> _waitingPlayers;

    public Player LocalPlayer => GetLocalPlayer();
    
    public int TakenSeatsAmount => _players.Count(x => x != null);

    [SerializeField] private float _conncetionLostCheckInterval;

    private void OnValidate()
    {
        if (_players.Count == MaxSeats && _waitingPlayers.Count == MaxSeats)
        {
            return;
        }

        _players = new List<Player>(MaxSeats);
        _waitingPlayers = new List<Player>();
        for (var i = 0; i < MaxSeats; i++)
        {
            _players.Add(null);
            _waitingPlayers.Add(null);
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
        if (_players[seatNumber] != null || _waitingPlayers[seatNumber] != null)
        {
            Log.WriteToFile($"Player ('{player.NickName}') can`t take the {seatNumber} seat, its already taken by some Player.'");
            return false;
        }
        
        TryLeave(player);

        if (Game.Instance.IsPlaying == true && _players.Contains(player) == false)
        {
            _waitingPlayers[seatNumber] = player;
            PlayerWaitForSitEvent?.Invoke(player, seatNumber);
            return false;
        }
        
        _players[seatNumber] = player;

        Log.WriteToFile($"Player ('{player.NickName}') sit on {seatNumber} seat.");

        PlayerSitEvent?.Invoke(player, seatNumber);
        return true;
    }
    
    public bool TryLeave(Player player)
    {
        if (_players.Contains(player) == false && _waitingPlayers.Contains(player) == false)
        {
            return false;
        }

        int seatNumber = -1;

        if (_players.Contains(player) == true)
        {
            seatNumber = _players.IndexOf(player);
            _players[seatNumber] = null;
        }
        else
        {
            seatNumber = _waitingPlayers.IndexOf(player);
            _waitingPlayers[seatNumber] = null;
        }
        
        Log.WriteToFile($"Player ('{player.NickName}') leave from {seatNumber} seat.");

        PlayerLeaveEvent?.Invoke(player, seatNumber);
        return true;
    }
    
    public void SitEveryoneWaiting()
    {
        for (var i = 0; i < _waitingPlayers.Count; i++)
        {
            if (_waitingPlayers[i] == null)
            {
                continue;
            }

            if (_players.Contains(_waitingPlayers[i]) == true)
            {
                Log.WriteToFile($"THIS SHOULD NEVER HAPPENED!!! Player collection already contains some waiting player ('{_waitingPlayers[i].NickName}')");
                continue;
            }

            _players[i] = _waitingPlayers[i];
            _waitingPlayers[i] = null;
            
            PlayerSitEvent?.Invoke(_players[i], i);
        }
    }
    
    public bool IsFree(int seatNumber)
    {
        return _players[seatNumber] == null && _waitingPlayers[seatNumber] == null;
    }


    private Player GetLocalPlayer()
    {
        Player localPlayer = _players.FirstOrDefault(x => x != null && x.IsOwner == true);
        if (localPlayer == null)
        {
            localPlayer = _waitingPlayers.FirstOrDefault(x => x != null && x.IsOwner == true);
        }

        return localPlayer;
    }

    private IEnumerator CheckForConnectonLost()
    {
        while (true)
        {
            for (var i = 0; i < _players.Count; i++)
            {
                try
                {
                    GameObject gameObjectName = _players[i].gameObject;
                    //Log.WriteToFile($"Connection stable on '{_players[i].NickName}'. GameObject name '{gameObjectName}'");
                }
                catch (NullReferenceException)
                {
                    try
                    {
                        // Check for MissingReferenceException ("Kolhoz" because cant catch the real MissingReferenceException in build).
                        string nick = _players[i].NickName;
                        Log.WriteToFile($"Connection lost on player ('{nick}') on {i} seat.");
                        TryLeave(_players[i]);
                    }
                    catch (NullReferenceException) { }
                }
            }

            yield return new WaitForSeconds(_conncetionLostCheckInterval);
        }
    }
}
