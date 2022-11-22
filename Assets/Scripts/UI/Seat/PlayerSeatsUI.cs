using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerSeatsUI : MonoBehaviour
{
    public static PlayerSeatsUI Instance { get; private set; }

    public event Action<int> PlayerClickTakeButton;

    public List<SeatUI> Seats => _seatsUI.ToList();
    [ReadOnly]
    [SerializeField] private List<SeatUI> _seatsUI;

    private readonly List<Vector3> _defaultSeatPositions = new(); 
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    private void OnEnable()
    {
        PlayerSeats.PlayerSitEvent += OnPlayerSit;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
    }

    private void OnDisable()
    {
        PlayerSeats.PlayerSitEvent -= OnPlayerSit;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;
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

        foreach (Vector3 seatPosition in _seatsUI.Select(x => x.transform.localPosition))
        {
            _defaultSeatPositions.Add(seatPosition);
        }
    }
        
    private void OnPlayerSit(Player player, int seatNumber)
    {
        StringBuilder stringBuilderAvatar = new();
        foreach (char symbol in player.AvatarBase64String)
        {
            stringBuilderAvatar.Append(symbol);
        }
        byte[] rawData = Convert.FromBase64String(stringBuilderAvatar.ToString());
        _seatsUI[seatNumber].PlayerImage.sprite = TextureConverter.GetSprite(rawData);
        _seatsUI[seatNumber].NickName.text = player.NickName;

        if (player.IsOwner == false)
        {
            return;
        }

        CenterPlayerSeat(seatNumber);
        SetupPocketCardsVisibility(seatNumber);
    }

    private void OnPlayerLeave(Player player, int seatNumber)
    {
        _seatsUI[seatNumber].PlayerImage.sprite = Resources.Load<Sprite>("Sprites/Arrow");
        _seatsUI[seatNumber].NickName.text = string.Empty;
    }

    private void CenterPlayerSeat(int centralSeatNubmer)
    {
        int[] centredIndexes = GetCentredIndexes(centralSeatNubmer);

        for (var newIndex = 0; newIndex < centredIndexes.Length; newIndex++)
        {
            int preveousIndex = centredIndexes[newIndex];
            _seatsUI[preveousIndex].transform.localPosition = _defaultSeatPositions[newIndex];
        }
        
        Log.WriteLine($"Changed central view to {centralSeatNubmer}.");
    }

    private int[] GetCentredIndexes(int centralSeatNubmer)
    {
        List<int> centredIndexes = new();

        for (var i = 0; i < PlayerSeats.MaxSeats; i++)
        {
            centredIndexes.Add((centralSeatNubmer + i) % PlayerSeats.MaxSeats);
        }

        return centredIndexes.ToArray();
    }

    private void SetupPocketCardsVisibility(int centralSeatNubmer)
    {
        foreach (SeatUI seatUI in _seatsUI)
        {
            seatUI.PocketCards.gameObject.SetActive(true);
        }

        _seatsUI[centralSeatNubmer].PocketCards.gameObject.SetActive(false);
    }

    // Button.
    private void OnPlayerClickTakeButton(int seatNumber)
    {
        PlayerClickTakeButton?.Invoke(seatNumber);
    }
}
