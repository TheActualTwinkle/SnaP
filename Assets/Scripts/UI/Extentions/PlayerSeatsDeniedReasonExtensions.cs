using System;

public static class PlayerSeatsDeniedReasonExtensions
{
    public static string GetMessage(this PlayerSeats.SitDenyReason sitDenyReason)
    {
        return sitDenyReason switch
        {
            PlayerSeats.SitDenyReason.SeatOccupiedByOtherPlayer => "Seat occupied by other player!",
            PlayerSeats.SitDenyReason.StackTooSmall => "Stack is too small!",
            _ => throw new ArgumentOutOfRangeException(nameof(sitDenyReason), sitDenyReason, null)
        };
    }
}
