using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSeatsUI : MonoBehaviour
{
    public static PlayerSeatsUI Instance { get; private set; }

    public event Action<int> PlayerClickTakeButton;

    public List<SeatUI> Seats => _seatsUI.ToList();
    [ReadOnly] [SerializeField] private List<SeatUI> _seatsUI;

    private readonly List<Vector3> _defaultSeatPositions = new();

    [Range(0f, 1f)] [SerializeField] private float _waitingTransparecyAlpha;
    
    private static Game Game => Game.Instance;
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Betting Betting => Betting.Instance;

    private void OnEnable()
    {
        PlayerSeats.PlayerSitEvent += OnPlayerSit;
        PlayerSeats.PlayerWaitForSitEvent += OnPlayerWaitForSit;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;
    }

    private void OnDisable()
    {
        PlayerSeats.PlayerSitEvent -= OnPlayerSit;
        PlayerSeats.PlayerWaitForSitEvent -= OnPlayerWaitForSit;
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
        _seatsUI = GetComponentsInChildren<SeatUI>().ToList();
    }

    private void Start()
    {
        foreach (Vector3 seatPosition in _seatsUI.Select(x => x.transform.localPosition))
        {
            _defaultSeatPositions.Add(seatPosition);
        }
    }
        
    private void OnPlayerSit(Player player, int seatNumber)
    {
        byte[] rawData = Convert.FromBase64String(player.AvatarBase64String);
        _seatsUI[seatNumber].PlayerImage.sprite = TextureConverter.GetSprite(rawData);
        _seatsUI[seatNumber].NickNameText.text = player.NickName;
        _seatsUI[seatNumber].NickNameBackgroundImage.enabled = true;
        
        ChanageSeatImageTransparency(seatNumber, 1f);

        if (player.IsOwner == false)
        {
            return;
        }

        CenterPlayerSeat(seatNumber);
        SetupPocketCardsVisibility(seatNumber);
    }

    private void OnPlayerWaitForSit(Player player, int seatNumber)
    {
        OnPlayerSit(player, seatNumber);
        ChanageSeatImageTransparency(seatNumber, _waitingTransparecyAlpha);
    }
    
    private void OnPlayerLeave(Player player, int seatNumber)
    {
        _seatsUI[seatNumber].PlayerImage.sprite = Resources.Load<Sprite>("Sprites/Arrow");
        _seatsUI[seatNumber].NickNameText.text = string.Empty;
        _seatsUI[seatNumber].NickNameBackgroundImage.enabled = false; 

        ChanageSeatImageTransparency(seatNumber, 1f);
    }
    
    private void ChanageSeatImageTransparency(int seatNumber, float alpha)
    {
        Color baseColor = _seatsUI[seatNumber].PlayerImage.color;
        Color newColor = new(baseColor.r, baseColor.g, baseColor.b, alpha);
        _seatsUI[seatNumber].PlayerImage.color = newColor;
    }
    
    private void CenterPlayerSeat(int centralSeatNubmer)
    {
        int[] centredIndexes = GetCentredIndexes(centralSeatNubmer);

        for (var newIndex = 0; newIndex < centredIndexes.Length; newIndex++)
        {
            int preveousIndex = centredIndexes[newIndex];
            _seatsUI[preveousIndex].transform.localPosition = _defaultSeatPositions[newIndex];
        }
        
        Log.WriteToFile($"Changed central view to {centralSeatNubmer}.");
    }

    private static int[] GetCentredIndexes(int centralSeatNubmer)
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
