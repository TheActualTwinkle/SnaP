using System;

/// <summary>
/// Source https://github.com/ccqi/TexasHoldem
/// </summary>
[System.Serializable]
public class CardObject
{
    public Suit Suit { get; }

    public Value Value { get; }

    public CardObject(Suit suit, Value value)
    {
        Suit = suit;
        Value = value;
    }
    
    public static string RankToString(int rank)
    {
        return ((Value)rank).ToString();
    }
    
    public int GetRank()
    {
        return (int)Value;
    }


    #region Overrides

    public static bool operator ==(CardObject a, CardObject b)
    {
        return a.Value == b.Value;
    }
    
    public static bool operator !=(CardObject a, CardObject b)
    {
        return a.Value != b.Value;
    }
    
    public static bool operator <(CardObject a, CardObject b)
    {
        return a.Value < b.Value;
    }
    
    public static bool operator >(CardObject a, CardObject b)
    {
        return a.Value > b.Value;
    }
    
    public static bool operator <=(CardObject a, CardObject b)
    {
        return a.Value <= b.Value;
    }
    
    public static bool operator >=(CardObject a, CardObject b)
    {
        return a.Value >= b.Value;
    }

    protected bool Equals(CardObject other)
    {
        return Suit == other.Suit && Value == other.Value;
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

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((CardObject)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Suit, (int)Value);
    }
    
    #endregion
}
