using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Board
{
    public List<CardObject> Cards => _cards.ToList();
    [SerializeField] private List<CardObject> _cards;

    public Board(List<CardObject> cards)
    {
        _cards = cards;
    }
}