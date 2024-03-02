using System;
using UnityEngine;

[RequireComponent(typeof(SdtConnectionResultUI))]
public class SdtConnectionResultAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    private SdtConnectionResultUI _sdtUi;

    private void Awake()
    {
        _sdtUi = GetComponent<SdtConnectionResultUI>();
    }

    public void Set()
    {
        SdtConnectionResultAddressablesLoader loader = AddressablesLoaderFactory.Get<SdtConnectionResultAddressablesLoader>();
        _sdtUi.SetSprites(loader.Sprites);
    }
}