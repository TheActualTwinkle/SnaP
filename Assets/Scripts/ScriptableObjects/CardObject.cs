using UnityEngine;

[System.Serializable]
public class CardObject
{
    public Suit Suit => _suit;
    [SerializeField] private Suit _suit;

    public Value Value => _value;
    [SerializeField] private Value _value;

    public CardObject(Suit suit, Value value)
    {
        _suit = suit;
        _value = value;
    }
}
