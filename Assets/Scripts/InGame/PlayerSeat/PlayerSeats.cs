using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerSeats : MonoBehaviour
{
    public enum SitDenyReason
    {
        SeatOccupiedByOtherPlayer,
        StackTooSmall,
    }
    
    public enum SeatLeaveReason
    {
        UserInput,
        Kick,
        ChangeSeat
    }

    public static PlayerSeats Instance { get; private set; }

    public const int MaxSeats = 5;

    public event Action<Player, int> PlayerSitEvent;
    public event Action<Player, int> PlayerWaitForSitEvent;
    public event Action<Player, int> PlayerLeaveEvent;
    public event Action<SitDenyReason, int> PlayerSitDeniedEvent;

    public List<Player> Players => _players.ToList();
    [ReadOnly] [SerializeField] private List<Player> _players;

    public List<Player> WaitingPlayers => _waitingPlayers.ToList();
    [ReadOnly] [SerializeField] private List<Player> _waitingPlayers;

    public Player LocalPlayer => GetLocalPlayer();

    public int PlayersAmount => _players.Count(x => x != null);

    public int WaitingPlayersAmount => _waitingPlayers.Count(x => x != null);

    [SerializeField] private float _connectionLostCheckInterval;
    
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
#if !UNITY_EDITOR
        StartCoroutine(CheckForConnectionLost());
#endif
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.H))
        {
            foreach (Player player in Players)
            {
                Debug.Log($"Id: {player.OwnerClientId}. {player.PocketCard1} and {player.PocketCard2}");
            }
        }
    }

    public bool TryTake(Player player, int seatNumber, bool forceToSeat = false)
    {
        if (IsFree(seatNumber) == false)
        {
            PlayerSitDeniedEvent?.Invoke(SitDenyReason.SeatOccupiedByOtherPlayer, seatNumber);
            Logger.Log($"Player ({player}) can`t take the {seatNumber} seat, its already taken by Player ({player}).",
                Logger.LogLevel.Error);
            return false;
        }

        if (player.Stack < Betting.Instance.BigBlind)
        {
            PlayerSitDeniedEvent?.Invoke(SitDenyReason.StackTooSmall, seatNumber);
            Logger.Log($"Player ({player}) can`t take the {seatNumber} seat, stack smaller then Big blind.",
                Logger.LogLevel.Error);
            return false;
        }

        TryLeave(player, SeatLeaveReason.ChangeSeat);

        if (Game.Instance.IsPlaying == false || forceToSeat == true)
        {
            _players[seatNumber] = player;

            Logger.Log($"Player ({player}) sit on {seatNumber} seat.");

            PlayerSitEvent?.Invoke(player, seatNumber);
            return true;
        }

        _waitingPlayers[seatNumber] = player;
        PlayerWaitForSitEvent?.Invoke(player, seatNumber);
        return false;
    }

    public bool TryLeave(Player player, SeatLeaveReason leaveReason)
    {
        if (_players.Contains(player) == false && _waitingPlayers.Contains(player) == false)
        {
            return false;
        }

        int seatNumber;

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

        switch (leaveReason)
        {
            case SeatLeaveReason.UserInput:
            case SeatLeaveReason.ChangeSeat:
                Logger.Log($"Player ({player}) leave from {seatNumber} seat.");
                break;
            case SeatLeaveReason.Kick:
                Logger.Log($"Player ({player}) kicked from {seatNumber} seat.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(leaveReason), leaveReason, null);
        }
        
        PlayerLeaveEvent?.Invoke(player, seatNumber);
        return true;
    }

    public void SitEveryoneWaiting()
    {
        for (var i = 0; i < _waitingPlayers.Count; i++)
        {
            if (_waitingPlayers[i] == null || _waitingPlayers[i].Stack == 0)
            {
                continue;
            }

            if (_players.Contains(_waitingPlayers[i]) == true)
            {
                Logger.Log(
                    $"THIS SHOULD NEVER HAPPENED!!! Player collection already contains some waiting player ({_waitingPlayers[i]}).",
                    Logger.LogLevel.Error);
                continue;
            }

            _players[i] = _waitingPlayers[i];
            _waitingPlayers[i] = null;

            PlayerSitEvent?.Invoke(_players[i], i);
        }
    }

    public void KickPlayersWithZeroStack()
    {
        for (var i = 0; i < _players.Count; i++)
        {
            Player player = _players[i];

            if (player == null || player.Stack != 0)
            {
                continue;
            }

            _players[i] = null;
            _waitingPlayers[i] = player;
            PlayerWaitForSitEvent?.Invoke(player, i);
        }
    }

    private bool IsFree(int seatNumber)
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

    // ReSharper disable once UnusedMember.Local
    // Check for MissingReferenceException SHIT METHOD because Unity`s NGO doesn`t provide working API for clients disconnecting.
    private IEnumerator CheckForConnectionLost()
    {
        while (true)
        {
            for (var i = 0; i < _players.Count; i++)
            {
                try
                {
                    GameObject checkGameObject = _players[i].gameObject;
                }
                catch (NullReferenceException)
                {
                    try
                    {
                        string nick = _players[i].NickName;
                        Logger.Log($"Connection lost on player ('{_players[i]}') on {i} seat.", Logger.LogLevel.Error);
                        TryLeave(_players[i], SeatLeaveReason.Kick);
                        
                        continue;
                    }
                    catch (NullReferenceException) { }
                }

                try
                {
                    GameObject checkGameObject = _waitingPlayers[i].gameObject;
                }
                catch (NullReferenceException)
                {
                    try
                    {
                        string nick = _waitingPlayers[i].NickName;
                        Logger.Log($"Connection lost on player ('{_waitingPlayers[i]}') on {i} seat.", Logger.LogLevel.Warning);
                        TryLeave(_waitingPlayers[i], SeatLeaveReason.Kick);
                    }
                    catch (NullReferenceException) { }
                }
            }
            
            yield return new WaitForSeconds(_connectionLostCheckInterval);
        }
    }
}