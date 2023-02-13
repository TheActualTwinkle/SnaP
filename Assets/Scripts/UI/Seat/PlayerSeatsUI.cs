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
        Game.EndDealEvent += OnEndGame;

        PlayerSeats.PlayerSitEvent += OnPlayerSit;
        PlayerSeats.PlayerWaitForSitEvent += OnPlayerWaitForSit;
        PlayerSeats.PlayerLeaveEvent += OnPlayerLeave;

        Betting.PlayerEndBettingEvent += PlayerEndBetting;
    }

    private void OnDisable()
    {
        Game.EndDealEvent -= OnEndGame;
        
        PlayerSeats.PlayerSitEvent -= OnPlayerSit;
        PlayerSeats.PlayerWaitForSitEvent -= OnPlayerWaitForSit;
        PlayerSeats.PlayerLeaveEvent -= OnPlayerLeave;

        Betting.PlayerEndBettingEvent += PlayerEndBetting;
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

    private void OnEndGame(WinnerInfo winnerInfo)
    {
        Player winner = PlayerSeats.Players.First(x => x != null && x.OwnerClientId == winnerInfo.WinnerId);

        if (winner == null)
        {
            return;
        }
        
        int index = PlayerSeats.Players.IndexOf(winner);
        
        _seatsUI[index].StackText.text = winner.Stack.ToString();
    }
        
    private void OnPlayerSit(Player player, int seatNumber)
    {
        byte[] rawData = Convert.FromBase64String(player.AvatarBase64String);
        _seatsUI[seatNumber].PlayerImage.sprite = TextureConverter.GetSprite(rawData);
        _seatsUI[seatNumber].NickNameText.text = player.NickName;
        _seatsUI[seatNumber].StackText.text = player.Stack.ToString();
        _seatsUI[seatNumber].NickNameBackgroundImage.enabled = true;
        _seatsUI[seatNumber].StackBackgroundImage.enabled = true;
        
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
        _seatsUI[seatNumber].StackText.text = string.Empty;
        _seatsUI[seatNumber].NickNameBackgroundImage.enabled = false; 
        _seatsUI[seatNumber].StackBackgroundImage.enabled = false;

        ChanageSeatImageTransparency(seatNumber, 1f);
    }

    private void PlayerEndBetting(BetActionInfo betActionInfo)
    {
        if (betActionInfo.BetAction is not (BetAction.Call or BetAction.Raise or BetAction.Bet or BetAction.AllIn))
        {
            return;
        }
        
        int index = PlayerSeats.Players.IndexOf(betActionInfo.Player);

        if (index == -1)
        {
            return;
        }
        
        _seatsUI[index].StackText.text = betActionInfo.Player.Stack.ToString();
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
