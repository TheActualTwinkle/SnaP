using UnityEngine;

[RequireComponent(typeof(SfxAudioPlayer))]
public class SfxAudioAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    [SerializeField] private SfxAudioPlayer _sfxAudioPlayer;
    
    public void Set()
    {
        SfxAudioAddressablesLoader loader = AddressablesLoaderFactory.Get<SfxAudioAddressablesLoader>();
        _sfxAudioPlayer.SetClips(loader.Clips);
    }
}