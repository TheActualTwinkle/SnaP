using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerSeatsDeniedReasonExtensions
{
    public static string GetMessage(this PlayerSeats.DeniedReason deniedReason)
    {
        return deniedReason switch
        {
            PlayerSeats.DeniedReason.SeatOccupiedByOtherPlayer => "Seat occupied by other player!",
            PlayerSeats.DeniedReason.StackTooSmall => "Stack is too small!",
            _ => throw new ArgumentOutOfRangeException(nameof(deniedReason), deniedReason, null)
        };
    }
}
