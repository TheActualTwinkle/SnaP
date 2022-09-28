using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Board
{
    public List<CardObject> Cards => _cards.ToList();
    [ReadOnly] private List<CardObject> _cards = new List<CardObject>(5);

    public Board(List<CardObject> cards)
    {
        _cards = cards;
    }
}