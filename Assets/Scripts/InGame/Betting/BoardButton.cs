using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BoardButton
{
    public static event Action<int> OnMove; 

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
        List<Player> players = PlayerSeats.Players;
        for (var i = 0; i < players.Count; i++)
        {
            if (players[i] != null)
            {
                indexes.Add(i);
            }
        }

        return indexes.ToArray();
    }
}