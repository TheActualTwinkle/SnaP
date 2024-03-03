using UnityEngine;

[RequireComponent(typeof(IChipsAssetUser))]
public class ChipsAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    private IChipsAssetUser _assetUser;

    private void Awake()
    {
        _assetUser = GetComponent<IChipsAssetUser>();
    }

    public void Set()
    {
        ChipsAddressablesLoader loader = AddressablesLoaderFactory.Get<ChipsAddressablesLoader>();
        _assetUser.SetChipsSprites(loader.Sprites);
    }
}