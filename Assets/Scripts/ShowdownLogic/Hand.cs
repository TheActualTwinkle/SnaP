using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Source https://github.com/ccqi/TexasHoldem
/// </summary>
public class Hand
{
    private List<CardObject> _hand;
    private readonly List<int> _combinationId;

    public Hand(IEnumerable<CardObject> cards)
    {
        _hand = cards.ToList();
        _combinationId = new List<int>();
    }
    
    public Hand()
    {
        _hand = new List<CardObject>();
        _combinationId = new List<int>();
    }
    
    public Hand(Hand otherHand)
    {
        _hand = new List<CardObject>(otherHand._hand);
        _combinationId = new List<int>();
    }
    
    public CardObject this[int index] => _hand[index];

    public List<CardObject> GetMainCombination()
    {
        return _combinationId[0] switch
        {
            1 => new List<CardObject> {_hand[0]},
            2 => new List<CardObject> {_hand[0], _hand[1]},
            3 => new List<CardObject> {_hand[0], _hand[1], _hand[2], _hand[3]},
            4 => new List<CardObject> {_hand[0], _hand[1], _hand[2]},
            5 => new List<CardObject> {_hand[0], _hand[1], _hand[2], _hand[3], _hand[4]},
            6 => new List<CardObject> {_hand[0], _hand[1], _hand[2], _hand[3], _hand[4]},
            7 => new List<CardObject> {_hand[0], _hand[1], _hand[2], _hand[3], _hand[4]},
            8 => new List<CardObject> {_hand[0], _hand[1], _hand[2], _hand[3]},
            9 => new List<CardObject> {_hand[0], _hand[1], _hand[2], _hand[3], _hand[4]},
            _ => new List<CardObject> {_hand[0], _hand[1], _hand[2], _hand[3], _hand[4]},
        };
    }
    
    public CardObject GetCard(int index)
    {
        if (index >= _hand.Count)
            throw new ArgumentOutOfRangeException();
        return _hand[index];
    }

    public void Clear()
    {
        _hand.Clear();
        _combinationId.Clear();
    }
    
    public void Add(CardObject card)
    {
        _hand.Add(card);
    }
    
    public void RemoveAt(int index)
    {
        _hand.RemoveAt(index);
    }
    
    public void Remove(CardObject card)
    {
        _hand.Remove(card);
    }
    
    public void AddCombinationId(int value)
    {
        _combinationId.Add(value);
    }
    
    public int Count()
    {
        return _hand.Count;
    }

    public void SortByRank()
    {
        _hand = QuickSortRank(_hand);
    }
    
    private List<CardObject> QuickSortRank(List<CardObject> myCards)
    {
        Random ran = new();

        if (myCards.Count() <= 1)
        {
            return myCards;
        }
        
        CardObject pivot = myCards[ran.Next(myCards.Count())];
        myCards.Remove(pivot);

        List<CardObject> less = new();
        List<CardObject> greater = new();
        // Assign values to less or greater list
        foreach (CardObject i in myCards)
        {
            if (i > pivot)
            {
                greater.Add(i);
            }
            else if (i <= pivot)
            {
                less.Add(i);
            }
        }
        // Recurse for less and greaterlists
        List<CardObject> list = new();
        list.AddRange(QuickSortRank(greater));
        list.Add(pivot);
        list.AddRange(QuickSortRank(less));
        return list;
    }

    // ReSharper disable once UnusedMember.Local
    private List<CardObject> QuickSortSuit(List<CardObject> myCards)
    {
        Random ran = new();

        if (myCards.Count() <= 1)
        {
            return myCards;
        }
        
        CardObject pivot = myCards[ran.Next(myCards.Count())];
        myCards.Remove(pivot);

        List<CardObject> less = new();
        List<CardObject> greater = new();
        // Assign values to less or greater list
        for (var i = 0; i < myCards.Count(); i++)
        {
            if (myCards[i].Suit > pivot.Suit)
            {
                greater.Add(myCards[i]);
            }
            else if (myCards[i].Suit <= pivot.Suit)
            {
                less.Add(myCards[i]);
            }
        }
        
        // Recurse for less and greaterlists
        List<CardObject> list = new();
        list.AddRange(QuickSortSuit(less));
        list.Add(pivot);
        list.AddRange(QuickSortSuit(greater));
        return list;
    }

    #region Overrides

    public override string ToString()
    {
        if (_combinationId.Any() == false)
        {
            return "No Poker Hand is Found";
        }

        return _combinationId[0] switch
        {
            1 => CardObject.RankToString(_combinationId[1]) + " High. Others: " + string.Join(", ", _combinationId.Skip(2)),
            2 => "Pair of " + CardObject.RankToString(_combinationId[1]) + "`s. Others: " + string.Join(", ", _combinationId.Skip(2)),
            3 => "Two Pair: " + CardObject.RankToString(_combinationId[1]) + "s over " + CardObject.RankToString(_combinationId[2]) + "s. Kicker: " + CardObject.RankToString(_combinationId[3]),
            4 => "Three " + CardObject.RankToString(_combinationId[1]) + "`s. Others: " + string.Join(", ", _combinationId.Skip(2)),
            5 => CardObject.RankToString(_combinationId[1]) + " High Straight",
            6 => CardObject.RankToString(_combinationId[1]) + " High Flush. Kicker: " + _combinationId[2],
            7 => CardObject.RankToString(_combinationId[1]) + "`s Full of " + CardObject.RankToString(_combinationId[2]) + "s",
            8 => "Quad " + CardObject.RankToString(_combinationId[1]) + "s Kicker: " + _combinationId[2],
            9 => CardObject.RankToString(_combinationId[1]) + " High Straight Flush",
            _ => "Royal Flush"
        };
    }
    
    public static bool operator ==(Hand a, Hand b)
    {
        for (var i = 0; i < a!._combinationId.Count(); i++)
        {
            if (a._combinationId[i] != b!._combinationId[i])
            {
                return false;
            }
        }
        return true;
    }
    
    public static bool operator !=(Hand a, Hand b)
    {
        if (a == null | b == null)
        {
            throw new NullReferenceException();
        }
        
        if (a._combinationId.Count == 0 || b._combinationId.Count == 0)
            throw new NullReferenceException();
        for (var i = 0; i < a._combinationId.Count(); i++)
        {
            if (a._combinationId[i] != b._combinationId[i])
            {
                return true;
            }
        }
        return false;
    }
    
    public static bool operator <(Hand a, Hand b)
    {
        if (a._combinationId.Count == 0 || b._combinationId.Count == 0)
            throw new NullReferenceException();
        for (var i = 0; i < a._combinationId.Count(); i++)
        {
            if (a._combinationId[i] < b._combinationId[i])
            {
                return true;
            }
            if (a._combinationId[i] > b._combinationId[i])
            {
                return false;
            }
        }
        return false;
    }
    
    public static bool operator >(Hand a, Hand b)
    {
        if (a._combinationId.Count == 0 || b._combinationId.Count == 0)
            throw new NullReferenceException();
        for (var i = 0; i < a._combinationId.Count; i++)
        {
            if (a._combinationId[i] > b._combinationId[i])
            {
                return true;
            }
            if (a._combinationId[i] < b._combinationId[i])
            {
                return false;
            }

        }
        return false;
    }
    
    public static bool operator <=(Hand a, Hand b)
    {
        if (a._combinationId.Count == 0 || b._combinationId.Count == 0)
            throw new NullReferenceException();
        for (var i = 0; i < a._combinationId.Count; i++)
        {
            if (a._combinationId[i] < b._combinationId[i])
            {
                return true;
            }
            if (a._combinationId[i] > b._combinationId[i])
            {
                return false;
            }

        }
        return true;
    }
    
    public static bool operator >=(Hand a, Hand b)
    {
        if (a._combinationId.Count == 0 || b._combinationId.Count == 0)
            throw new NullReferenceException();
        for (var i = 0; i < a._combinationId.Count(); i++)
        {
            if (a._combinationId[i] > b._combinationId[i])
            {
                return true;
            }
            if (a._combinationId[i] < b._combinationId[i])
            {
                return false;
            }

        }
        return true;
    }
    
    public static Hand operator +(Hand a, Hand b)
    {
        for (var i = 0; i < b.Count(); i++)
        {
            a.Add(b[i]);
        }
        return a;
    }

    private bool Equals(Hand other)
    {
        return Equals(_hand, other._hand) && Equals(_combinationId, other._combinationId);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Hand)obj);
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return HashCode.Combine(_hand, _combinationId);
    }

    #endregion
}