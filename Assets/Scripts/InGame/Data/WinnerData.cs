using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WinnerData
{
    public readonly Player Winner;
    public readonly uint Pot;

    public WinnerData(Player winner, uint pot)
    {
        Winner = winner;
        Pot = pot;
    }
}
