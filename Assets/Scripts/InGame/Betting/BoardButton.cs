using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BoardButton
{
    public static event Action<int> OnMove;

    public static List<int> TurnSequensce { get; private set; }

    private const int EmptyPosition = -1;

    [SerializeField] private int _position = EmptyPosition;

    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Betting Betting => Betting.Instance;
    
    public void Move()
    {
        if (_position == EmptyPosition)
        {
            _position = GetActivePlayerIndexes().First();
        }

        int[] turnSequensce = GetTurnSequence();

        _position = turnSequensce.First();
        
        OnMove?.Invoke(_position);
    }

    public int[] GetTurnSequence()
    {
        List<int> activeSeatIndexes = GetActivePlayerIndexes().ToList();

        int pivot = activeSeatIndexes.IndexOf(_position);
        
        List<int> turnSequence = new();
        for (int i = pivot + 1; i < activeSeatIndexes.Count; i++)
        {
            turnSequence.Add(activeSeatIndexes[i]);
        }

        for (var i = 0; i <= pivot; i++)
        {
            turnSequence.Add(activeSeatIndexes[i]);
        }

        TurnSequensce = turnSequence.ToList();
        return turnSequence.ToArray();
    }

    public int[] GetPreflopTurnSequence()
    {
        List<int> turnSequence = GetTurnSequence().ToList();
        return GetSwapedByPivotTurnSequence(2, turnSequence); // 2 is because Big and Small blinds.
    }

    public int[] GetShowdownTurnSequence()
    {
        int[] turnSequence = GetTurnSequence();

        if (Betting.LastBetRaiser == null)
        {
            return turnSequence;
        }

        Player lastBetRaiser = Betting.Instance.LastBetRaiser;
        int lastBetRaiserIndex = PlayerSeats.Players.IndexOf(lastBetRaiser);
        int index = turnSequence.ToList().IndexOf(lastBetRaiserIndex);
        
        return GetSwapedByPivotTurnSequence(index, turnSequence);
    }

    private int[] GetSwapedByPivotTurnSequence(int pivot, IReadOnlyList<int> sequence)
    {
        List<int> splitedTurnSequence = new();
        for (int i = pivot; i < sequence.Count; i++)
        {
            splitedTurnSequence.Add(sequence[i]);
        }

        for (var i = 0; i < pivot; i++)
        {
            splitedTurnSequence.Add(sequence[i]);
        }

        return splitedTurnSequence.ToArray();
    } 
    
    private int[] GetActivePlayerIndexes()
    {
        List<int> indexes = new();

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