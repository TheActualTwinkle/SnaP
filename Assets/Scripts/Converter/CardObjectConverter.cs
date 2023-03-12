using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public static class CardObjectConverter
{
    private const char Separator = ';';
    
    public static IEnumerable<CardObject> GetCards(IEnumerable<int> codedCards)
    {
        Queue<CardObject> cards = new();
        foreach (int card in codedCards)
        {
            Value value = (Value)(card % 100);
            Suit suit = (Suit)((card - (int)value) / 100);
            cards.Enqueue(new CardObject(suit, value));
        }

        return cards;
    }

    public static IEnumerable<CardObject> GetCards(string codedCards)
    {
        string[] stringCards = codedCards.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        return GetCards(stringCards.Select(int.Parse));
    }

    public static int[] GetCodedCards(IEnumerable<CardObject> cards) // e.g. 211 means Suit = 2, Value = 11 that`s mean Jack of Spades. 
    {
        List<int> codedCards = new();
        foreach (CardObject card in cards)
        {
            var suit = (int)card.Suit;
            var value = (int)card.Value;
            codedCards.Add(suit * 100 + value);
        }

        return codedCards.ToArray();
    }    
    
    public static string GetCodedCardsString(IEnumerable<CardObject> cards) // e.g. 211 means Suit = 2, Value = 11 that`s mean Jack of Spades. 
    {
        var codedCards = string.Empty;
        foreach (CardObject card in cards)
        {
            var suit = (int)card.Suit;
            var value = (int)card.Value;
            codedCards += (suit * 100 + value) + ";";
        }

        return codedCards;
    }
}
