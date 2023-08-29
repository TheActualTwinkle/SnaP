using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

[Serializable]
public class BoardButton : NetworkBehaviour
{
    public static BoardButton Instance { get; private set; }

    public const int EmptyPosition = -1;

    public readonly NetworkVariable<int> PositionNetworkVariable = new(EmptyPosition);

    private static PlayerSeats PlayerSeats => PlayerSeats.Instance;
    private static Betting Betting => Betting.Instance;

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
    }

    public void Move()
    {
        if (IsServer == false)
        {
            return;
        }

        if (PositionNetworkVariable.Value == EmptyPosition)
        {
            PositionNetworkVariable.Value = GetActivePlayerIndexes().First();
        }

        PositionNetworkVariable.Value = GetTurnSequence().First();
    }

    public int[] GetTurnSequence()
    {
        List<int> activeSeatIndexes = GetActivePlayerIndexes().ToList();

        int pivot = activeSeatIndexes.IndexOf(PositionNetworkVariable.Value);

        List<int> turnSequence = new();
        for (int i = pivot + 1; i < activeSeatIndexes.Count; i++)
        {
            turnSequence.Add(activeSeatIndexes[i]);
        }

        for (var i = 0; i <= pivot; i++)
        {
            turnSequence.Add(activeSeatIndexes[i]);
        }

        return turnSequence.ToArray();
    }

    public int[] GetPreflopTurnSequence()
    {
        List<int> turnSequence = GetTurnSequence().ToList();
        return GetSwappedByPivotTurnSequence(2, turnSequence); // 2 is because Big and Small blinds.
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

        return GetSwappedByPivotTurnSequence(index, turnSequence);
    }

    private int[] GetSwappedByPivotTurnSequence(int pivot, IReadOnlyList<int> sequence)
    {
        List<int> splittedTurnSequence = new();
        for (int i = pivot; i < sequence.Count; i++)
        {
            splittedTurnSequence.Add(sequence[i]);
        }

        for (var i = 0; i < pivot; i++)
        {
            splittedTurnSequence.Add(sequence[i]);
        }

        return splittedTurnSequence.ToArray();
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