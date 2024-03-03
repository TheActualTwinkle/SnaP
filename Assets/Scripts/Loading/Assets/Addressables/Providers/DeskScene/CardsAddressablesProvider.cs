using UnityEngine;

[RequireComponent(typeof(ICardsAssetUser))]
public class CardsAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    private ICardsAssetUser _assetUser;

    private void Awake()
    {
        _assetUser = GetComponent<ICardsAssetUser>();
    }

    public void Set()
    {
        CardsAddressablesLoader loader = AddressablesLoaderFactory.Get<CardsAddressablesLoader>();
        _assetUser.SetCardsSprites(loader.Sprites);
    }
}