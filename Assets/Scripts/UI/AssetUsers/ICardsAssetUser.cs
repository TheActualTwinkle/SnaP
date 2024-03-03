using System.Collections.Generic;
using UnityEngine;

public interface ICardsAssetUser
{
    void SetCardsSprites(IEnumerable<Sprite> sprites);
}