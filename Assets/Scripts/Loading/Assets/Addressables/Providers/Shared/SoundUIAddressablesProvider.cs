using UnityEngine;

[RequireComponent(typeof(SoundUI))]
public class SoundUIAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    [SerializeField] private SoundUI _soundUI;
    
    public void Set()
    {
        SoundUIAddressablesLoader loader = AddressablesLoaderFactory.Get<SoundUIAddressablesLoader>();
        _soundUI.SetSprites(loader.MusicSprite, loader.CrossSprite);
    }
}