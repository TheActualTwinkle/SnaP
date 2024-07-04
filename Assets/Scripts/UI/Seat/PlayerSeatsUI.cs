using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSeatsUI : MonoBehaviour
{
    public static event Action InstantiatedEvent; 
    
    public static PlayerSeatsUI Instance { get; private set; }

    public event Action<int> PlayerClickTakeButtonEvent;

    public List<SeatUI> Seats => _seatsUI.ToList();
    [ReadOnly] [SerializeField] private List<SeatUI> _seatsUI;

    private readonly List<Vector3> _defaultSeatPositions = new();

    [Range(0f, 1f)] [SerializeField] private float _waitingTransparencyAlpha;

    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    
    private IEnumerator _setPlayerAvatarImageWhenLoadedRoutine;

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
        
        InstantiatedEvent?.Invoke();
        
        FetchSeats();
    }

    private void FetchSeats()
    {
        for (var i = 0; i < PlayerSeats.Players.Count; i++)
        {
            if (PlayerSeats.Players[i] == null)
            {
                continue;
            }
            
            OnPlayerSit(PlayerSeats.Players[i], i);
        }

        for (var i = 0; i < PlayerSeats.WaitingPlayers.Count; i++)
        {
            if (PlayerSeats.WaitingPlayers[i] == null)
            {
                continue;
            }
            
            OnPlayerWaitForSit(PlayerSeats.WaitingPlayers[i], i);
        }
    }
        
    private void OnPlayerSit(Player player, int seatNumber)
    {
        _seatsUI[seatNumber].NickNameText.text = player.NickName;
        _seatsUI[seatNumber].NickNameBackgroundImage.enabled = true;
        
        ChangeSeatImageTransparency(seatNumber, 1f);

        _setPlayerAvatarImageWhenLoadedRoutine = SetPlayerAvatarImageWhenLoaded(player, seatNumber);
        StartCoroutine(_setPlayerAvatarImageWhenLoadedRoutine);
        
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
        ChangeSeatImageTransparency(seatNumber, _waitingTransparencyAlpha);
    }
    
    private void OnPlayerLeave(Player player, int seatNumber)
    {
        _seatsUI[seatNumber].PlayerImage.sprite = Resources.Load<Sprite>("Sprites/Arrow");
        _seatsUI[seatNumber].NickNameText.text = string.Empty;
        _seatsUI[seatNumber].NickNameBackgroundImage.enabled = false; 

        ChangeSeatImageTransparency(seatNumber, 1f);

        if (_setPlayerAvatarImageWhenLoadedRoutine != null)
        {
            StopCoroutine(_setPlayerAvatarImageWhenLoadedRoutine);
        }
    }
    
    private void ChangeSeatImageTransparency(int seatNumber, float alpha)
    {
        Color baseColor = _seatsUI[seatNumber].PlayerImage.color;
        Color newColor = new(baseColor.r, baseColor.g, baseColor.b, alpha);
        _seatsUI[seatNumber].PlayerImage.color = newColor;
    }
    
    private void CenterPlayerSeat(int centralSeatNumber)
    {
        int[] centredIndexes = GetCentredIndexes(centralSeatNumber);

        for (var newIndex = 0; newIndex < centredIndexes.Length; newIndex++)
        {
            int previousIndex = centredIndexes[newIndex];
            _seatsUI[previousIndex].transform.localPosition = _defaultSeatPositions[newIndex];
        }
        
        Logger.Log($"Changed central view to {centralSeatNumber}.");
    }

    private static int[] GetCentredIndexes(int centralSeatNumber)
    {
        List<int> centredIndexes = new();

        for (var i = 0; i < PlayerSeats.MaxSeats; i++)
        {
            centredIndexes.Add((centralSeatNumber + i) % PlayerSeats.MaxSeats);
        }

        return centredIndexes.ToArray();
    }

    private IEnumerator SetPlayerAvatarImageWhenLoaded(Player player, int seatNumber)
    {
        _seatsUI[seatNumber].EnableLoadingImage();
        
        yield return new WaitUntil(() => player.IsAvatarImageReady == true);
        
        _seatsUI[seatNumber].DisableLoadingImage();
        
        TrySetPlayerAvatarImage(player, seatNumber);
    }

    private bool TrySetPlayerAvatarImage(Player player, int seatNumber)
    {

        Image seatImage = _seatsUI[seatNumber].PlayerImage;
        var imageWidth = (int)seatImage.rectTransform.rect.width;
        var imageHeight = (int)seatImage.rectTransform.rect.height;
        
        _seatsUI[seatNumber].PlayerImage.sprite = TextureConverter.GetSprite(player.AvatarDto.CodedValue, imageWidth, imageHeight);
        return true;
    }
    
    private void SetupPocketCardsVisibility(int centralSeatNumber)
    {
        foreach (SeatUI seatUI in _seatsUI)
        {
            seatUI.PocketCards.gameObject.SetActive(true);
        }

        _seatsUI[centralSeatNumber].PocketCards.gameObject.SetActive(false);
    }

    // Button.
    private void OnPlayerClickTakeButton(int seatNumber)
    {
        PlayerClickTakeButtonEvent?.Invoke(seatNumber);
    }
}
