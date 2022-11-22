using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PocketCards : MonoBehaviour
{
    public List<CardObject> Cards => _cards.ToList();
    [ReadOnly] [SerializeField] private List<CardObject> _cards;

    public void SetPocketCards(CardObject card1, CardObject card2)
    {
        _cards[0] = card1;
        _cards[1] = card2;
    }
}