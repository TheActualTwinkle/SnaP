using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSeatsUI : MonoBehaviour
{
    public static PlayerSeatsUI Instance { get; private set; }

    public event Action<int> PlayerClickTakeButtonEvent;

    public List<SeatUI> Seats => _seatsUI.ToList();
    [ReadOnly] [SerializeField] private List<SeatUI> _seatsUI;

    private readonly List<Vector3> _defaultSeatPositions = new();

    [Range(0f, 1f)] [SerializeField] private float _waitingTransparencyAlpha;

    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    
    private IEnumerator _waitForPlayerAvatarImageLoadedRoutine;

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
        _seatsUI[seatNumber].NickNameText.text = player.NickName;
        _seatsUI[seatNumber].NickNameBackgroundImage.enabled = true;
        
        ChangeSeatImageTransparency(seatNumber, 1f);

        if (_waitForPlayerAvatarImageLoadedRoutine != null)
        {
            StopCoroutine(_waitForPlayerAvatarImageLoadedRoutine);
        }

        _waitForPlayerAvatarImageLoadedRoutine = WaitForPlayerAvatarImageLoaded(player);
        StartCoroutine(_waitForPlayerAvatarImageLoadedRoutine);
        
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

        if (_waitForPlayerAvatarImageLoadedRoutine != null)
        {
            StopCoroutine(_waitForPlayerAvatarImageLoadedRoutine);
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
        
        Debug.Log($"Changed central view to {centralSeatNumber}.");
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

    private IEnumerator WaitForPlayerAvatarImageLoaded(Player player)
    {
        int playerIndex = PlayerSeats.Players.IndexOf(player);
        _seatsUI[playerIndex].EnableLoadingImage();
        
        yield return new WaitUntil(() => player.IsAvatarImageReady == true);
        
        _seatsUI[playerIndex].DisableLoadingImage();
        
        TrySetPlayerAvatarImage(player);
    }

    private bool TrySetPlayerAvatarImage(Player player)
    {
        int indexOfPlayerSeat = PlayerSeats.Players.IndexOf(player);

        Image seatImage = _seatsUI[indexOfPlayerSeat].PlayerImage;
        var imageWidth = (int)seatImage.rectTransform.rect.width;
        var imageHeight = (int)seatImage.rectTransform.rect.height;
        return TrySetImage(indexOfPlayerSeat, TextureConverter.GetSprite(player.AvatarData.CodedValue, imageWidth, imageHeight));
    }

    private bool TrySetImage(int seatIndex, Sprite sprite)
    {
        if (seatIndex == -1)
        {
            return false;
        }
        
        _seatsUI[seatIndex].PlayerImage.sprite = sprite;
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
