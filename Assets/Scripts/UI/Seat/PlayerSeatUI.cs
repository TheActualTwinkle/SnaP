using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSeatUI : MonoBehaviour
{
    public static PlayerSeatUI Instance => _instance;
    private static PlayerSeatUI _instance;

    public event Action<int> PlayerClickJoinButton;

    [SerializeField] private PlayerSeats _playerSeats;

    public List<SeatUI> Seats => _seats.ToList();
    [ReadOnly]
    [SerializeField] private List<SeatUI> _seats;

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
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _seats = GetComponentsInChildren<SeatUI>().ToList();
    }

    private void OnPlayerSit(PlayerSeatData data)
    {
        Sprite avatar = data.Player.Avatar;
        if (avatar == null)
        {
            avatar = Resources.Load<Sprite>("Sprites/Clown");
        }

        _seats[data.SeatNumber].PlayerImage.sprite = avatar;
    }

    private void OnPlayerLeave(PlayerSeatData data)
    {
        _seats[data.SeatNumber].PlayerImage.sprite = Resources.Load<Sprite>("Sprites/Arrow");
    }

    // Button.
    private void OnJoinButtonClick(int seatNumber)
    {
        PlayerClickJoinButton?.Invoke(seatNumber);
    }
}
