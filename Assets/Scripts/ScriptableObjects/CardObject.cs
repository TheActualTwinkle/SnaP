using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardObject
{
    public Values Value => _value;
    private Values _value;

    public Suits Suit => _suit;
    private Suits _suit;

    public Sprite SpriteFront => _spriteFront;
    private Sprite _spriteFront;

    public Sprite SpriteBack => _spriteBack;
    private Sprite _spriteBack;

    public CardObject(Values value, Suits suit)
    {
        _value = value;
        _suit = suit;
    }

    public void Initialize()
    {
        string valueId = ((int)_value + 2).ToString();
        string suitId = _suit.ToString();

        _spriteFront = Resources.Load<Sprite>($"Sprites/{valueId}_{suitId}");
        _spriteBack = Resources.Load<Sprite>($"Sprites/BlueCardBack");
    }
}
