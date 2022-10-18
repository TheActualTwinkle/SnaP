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

public class PlayerSeatUI : MonoBehaviour
{
    public static PlayerSeatUI Instance => _instance;
    private static PlayerSeatUI _instance;

    public event Action<int> PlayerClickTakeButton;

    public List<SeatUI> Seats => _seatsUI.ToList();
    [ReadOnly]
    [SerializeField] private List<SeatUI> _seatsUI;

    private List<Vector3> _defaultSeatPositions = new List<Vector3>(); 

    private void OnEnable()
    {
        PlayerSeats.Instance.PlayerSitEvent += OnPlayerSit;
        PlayerSeats.Instance.PlayerLeaveEvent += OnPlayerLeave;
    }

    private void OnDisable()
    {
        PlayerSeats.Instance.PlayerSitEvent -= OnPlayerSit;
        PlayerSeats.Instance.PlayerLeaveEvent -= OnPlayerLeave;
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

        _seatsUI = GetComponentsInChildren<SeatUI>().ToList();
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
        Debug.Log($"Changed central view to {centralSeatNubmer}");

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

        for (int i = 0; i < PlayerSeats.MAX_SEATS; i++)
        {
            centredIndexes.Add((centralSeatNubmer + i) % PlayerSeats.MAX_SEATS);
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
