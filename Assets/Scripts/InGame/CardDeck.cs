using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

[Serializable]
public class CardDeck
{
    public List<CardObject> Cards => _cards.ToList();
    private Queue<CardObject> _cards;
    
    public CardDeck()
    {
        Create();
        Shuffle();
    }

    public CardDeck(IEnumerable<int> cards)
    {
        _cards = new Queue<CardObject>();
        foreach (int card in cards)
        {
            Value value = (Value)(card % 100);
            Suit suit = (Suit)((card - (int)value) / 100);
            _cards.Enqueue(new CardObject(suit, value));
        }
    }

    public CardObject PullCard()
    {
        return _cards.Dequeue();
    }

    public CardObject[] PullCards(int amount)
    {
        List<CardObject> cards = new();
        for (var i = 0; i < amount; i++)
        {
            cards.Add(_cards.Dequeue());
        }

        return cards.ToArray();
    }
    
    private void Create()
    {
        _cards = new Queue<CardObject>();
        for (var i = 0; i < Enum.GetValues(typeof(Suit)).Length; i++)
        {
            for (var j = 2; j < Enum.GetValues(typeof(Value)).Length + 2; j++)
            {
                CardObject cardObject = new((Suit)i, (Value)j);
                _cards.Enqueue(cardObject);
            }
        }
    }
    
    private void Shuffle()
    {
        IOrderedEnumerable<CardObject> newCards = _cards.OrderBy(_ => new Random().Next(0, _cards.Count));
        _cards = new Queue<CardObject>(newCards);
    }
}