using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[System.Serializable]
public class CardDeck
{
    private Queue<CardObject> _cards;
    
    public CardDeck()
    {
        Create();
        Shuffle();
    }

    public CardDeck(int[] cards)
    {
        _cards = new Queue<CardObject>();
        foreach (int card in cards)
        {
            Value value = (Value)(card % 100);
            Suit suit = (Suit)((card - (int)value) / 100);
            _cards.Enqueue(new CardObject(suit, value));
        }
    }

    public int[] GetCodedCards() // e.g. 209 means Suit = 2, Value = 9 that`s mean Jack of Spades. 
    {
        List<int> codedCards = new();
        foreach (CardObject card in _cards)
        {
            var suit = (int)card.Suit;
            var value = (int)card.Value;
            codedCards.Add(suit * 100 + value);
        }

        return codedCards.ToArray();
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
            for (var j = 0; j < Enum.GetValues(typeof(Value)).Length; j++)
            {
                CardObject cardObject = new((Suit)i, (Value)j);

                _cards.Enqueue(cardObject);
            }
        }
    }
    
    private void Shuffle()
    {
        IOrderedEnumerable<CardObject> newCards = (_cards.OrderBy(x => new Random().Next(0, _cards.Count)));
        _cards = new Queue<CardObject>(newCards);
    }
}