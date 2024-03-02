using UnityEngine;

[RequireComponent(typeof(MusicAudio))]
public class MusicAudioAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    [SerializeField] private MusicAudio _musicAudio;
    
    public void Set()
    {
        MusicAudioAddressablesLoader loader = AddressablesLoaderFactory.Get<MusicAudioAddressablesLoader>();
        _musicAudio.SetClips(loader.Clips);
    }
}