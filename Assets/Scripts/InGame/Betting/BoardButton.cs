using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BoardButton
{
    public static event Action<int> OnMove;

    public static List<int> TurnSequensce { get; private set; }

    private const int EmptyPosition = -1;

    [SerializeField] private int _position = EmptyPosition;

    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    
    public void Move()
    {
        if (_position == EmptyPosition)
        {
            _position = GetActivePlayerIndexes().First();
        }

        int[] turnSequensce = GetTurnSequensce();

        _position = turnSequensce.First();
        
        OnMove?.Invoke(_position);
    }

    public int[] GetTurnSequensce()
    {
        List<int> activeSeatIndexes = GetActivePlayerIndexes().ToList();

        int pivot = activeSeatIndexes.IndexOf(_position);
        
        List<int> turnSequensce = new();
        for (int i = pivot + 1; i < activeSeatIndexes.Count; i++)
        {
            turnSequensce.Add(activeSeatIndexes[i]);
        }

        for (var i = 0; i <= pivot; i++)
        {
            turnSequensce.Add(activeSeatIndexes[i]);
        }

        TurnSequensce = turnSequensce.ToList();
        return turnSequensce.ToArray();
    }

    public int[] GetPreflopTurnSequensce()
    {
        List<int> turnSequensce = GetTurnSequensce().ToList();

        List<int> preflopTurnSequensce = new();
        for (var i = 2; i < turnSequensce.Count; i++)
        {
            preflopTurnSequensce.Add(turnSequensce[i]);
        }

        for (var i = 0; i < 2; i++)
        {
            preflopTurnSequensce.Add(turnSequensce[i]);
        }

        return preflopTurnSequensce.ToArray();
    }
    
    private int[] GetActivePlayerIndexes()
    {
        List<int> indexes = new();
        List<Player> players = PlayerSeats.Players.Where(x => x != null && x.BetAction != BetAction.Fold).ToList();

        for (var i = 0; i < PlayerSeats.Players.Count; i++)
        {
            Player player = PlayerSeats.Players[i];
            if (player != null && player.BetAction != BetAction.Fold)
            {
                indexes.Add(i);
            }
        }

        return indexes.ToArray();
    }
}