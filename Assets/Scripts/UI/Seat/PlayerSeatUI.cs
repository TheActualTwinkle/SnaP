using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerSeatUI : NetworkBehaviour
{
    public event Action<PlayerSeatData> PlayerClickJoinButton;

    [SerializeField] private Game _game;
    [SerializeField] private List<Transform> _seats;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        
    }

    // Button.
    private void OnJoinButtonClick(int seatNumber)
    {
        Player player = NetworkManager.LocalClient.PlayerObject.GetComponent<Player>();
        PlayerSeatData data = new PlayerSeatData(player, seatNumber);
        PlayerClickJoinButton?.Invoke(data);
    }
}
