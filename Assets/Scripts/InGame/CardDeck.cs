using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CardDeck
{
    private Queue<CardObject> _cards;

    public void Initialize()
    {
        _cards = new Queue<CardObject>();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                CardObject cardObject = new CardObject((Values)j, (Suits)i);
                cardObject.Initialize();

                _cards.Enqueue(cardObject);
            }
        }

        Shuffle();
    }

    public CardObject PullCard()
    {
        return _cards.Dequeue();
    }

    private void Shuffle()
    {
        _cards = new Queue<CardObject>(_cards.OrderBy(x => Random.Range(0, _cards.Count + 1)));
    }
}
