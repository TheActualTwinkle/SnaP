using UnityEngine;

[RequireComponent(typeof(BackgroundImageUI))]
public class BackgroundImageAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    private BackgroundImageUI _backgroundImageUI;

    private void Awake()
    {
        _backgroundImageUI = GetComponent<BackgroundImageUI>();
    }

    public void Set()
    {
        BackgroundImageAddressablesLoader loader = AddressablesLoaderFactory.Get<BackgroundImageAddressablesLoader>();
        _backgroundImageUI.SetSprite(loader.Sprite);
    }
}