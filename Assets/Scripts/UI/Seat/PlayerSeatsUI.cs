using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSeatsUI : MonoBehaviour
{
    public static PlayerSeatsUI Instance { get; private set; }

    public event Action<int> PlayerClickTakeButton;

    public List<SeatUI> Seats => _seatsUI.ToList();
    [ReadOnly]
    [SerializeField] private List<SeatUI> _seatsUI;

    private List<Vector3> _defaultSeatPositions = new List<Vector3>(); 
    private PlayerSeats _playerSeats => PlayerSeats.Instance;

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

    private void Awake()
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
        _seatsUI = GetComponentsInChildren<SeatUI>().ToList();

        foreach (var seatPosition in _seatsUI?.Select(x => x.transform.localPosition))
        {
            _defaultSeatPositions.Add(seatPosition);
        }
    }
        
    private void OnPlayerSit(Player player, int seatNumber)
    {
        _seatsUI[seatNumber].PlayerImage.sprite = player.Avatar;
        _seatsUI[seatNumber].NickName.text = player.NickName;

        if (player.IsOwner == true)
        {
            CenterPlayerSeat(seatNumber);
            SetupPocketCardsVisibility(seatNumber);
        }
    }

    private void OnPlayerLeave(Player player, int seatNumber)
    {
        _seatsUI[seatNumber].PlayerImage.sprite = Resources.Load<Sprite>("Sprites/Arrow");
        _seatsUI[seatNumber].NickName.text = string.Empty;
    }

    private void CenterPlayerSeat(int centralSeatNubmer)
    {
        Log.WriteLine($"Changed central view to {centralSeatNubmer}.");

        int[] centredIndexes = GetCentredIndexes(centralSeatNubmer);

        for (int newIndex = 0; newIndex < centredIndexes.Length; newIndex++)
        {
            int preveousIndex = centredIndexes[newIndex];
            _seatsUI[preveousIndex].transform.localPosition = _defaultSeatPositions[newIndex];
        }
    }

    private int[] GetCentredIndexes(int centralSeatNubmer)
    {
        List<int> centredIndexes = new List<int>();

        for (int i = 0; i < PlayerSeats.MaxSeats; i++)
        {
            centredIndexes.Add((centralSeatNubmer + i) % PlayerSeats.MaxSeats);
        }

        return centredIndexes.ToArray();
    }

    private void SetupPocketCardsVisibility(int centralSeatNubmer)
    {
        for (int i = 0; i < _seatsUI.Count; i++)
        {
            _seatsUI[i].PocketCards.gameObject.SetActive(true);
        }
        _seatsUI[centralSeatNubmer].PocketCards.gameObject.SetActive(false);
    }

    // Button.
    private void OnPlayerClickTakeButton(int seatNumber)
    {
        PlayerClickTakeButton?.Invoke(seatNumber);
    }
}
