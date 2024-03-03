using UnityEngine;

[RequireComponent(typeof(GameMainIcon))]
public class GameMainIconAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    [SerializeField] private GameMainIcon _icon;

    public void Set()
    {
        GameMainIconAddressablesLoader loader = AddressablesLoaderFactory.Get<GameMainIconAddressablesLoader>();
        _icon.SetSprite(loader.Sprite);
    }
}