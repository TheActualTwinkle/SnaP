using System;
using UnityEngine;

[RequireComponent(typeof(PopupTextSpawner))]
public class SitErrorPopup : MonoBehaviour
{
    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;

    [SerializeField] private int _seatNumber;
    [SerializeField] private PopupTextSpawner _popupTextSpawner;
    
    private void OnEnable()
    {
        PlayerSeats.PlayerSitDeniedEvent += OnPlayerSitDenied;
    }

    private void OnDisable()
    {
        PlayerSeats.PlayerSitDeniedEvent -= OnPlayerSitDenied;
    }

    private void OnPlayerSitDenied(PlayerSeats.DeniedReason deniedReason, int seatNumber)
    {
        if (seatNumber != _seatNumber)
        {
            return;
        }
        
        _popupTextSpawner.SetText(deniedReason.GetMessage());
        _popupTextSpawner.SpawnOnMousePosition();
    }
}
