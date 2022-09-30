using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSeatData
{
    public readonly Player Player;
    public readonly int SeatNumber;

    public PlayerSeatData(Player player, int seatNumber)
    {
        Player = player;
        SeatNumber = seatNumber;
    }
}
