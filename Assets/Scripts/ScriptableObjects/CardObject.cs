using System;using Unity.Netcode;

/// <summary>
/// Source https://github.com/ccqi/TexasHoldem
/// </summary>
[Serializable]
public class CardObject : INetworkSerializable
{
    public Suit Suit => _suit;
    private Suit _suit;

    public Value Value => _value;
    private Value _value;

    public CardObject() { }
    
    public CardObject(Suit suit, Value value)
    {
        _suit = suit;
        _value = value; 
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

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((CardObject)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Suit, (int)Value);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _suit);
        serializer.SerializeValue(ref _value);
    }

    #endregion
}
