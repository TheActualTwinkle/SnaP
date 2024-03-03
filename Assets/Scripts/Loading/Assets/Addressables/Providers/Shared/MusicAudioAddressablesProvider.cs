using UnityEngine;

[RequireComponent(typeof(MusicAudioPlayer))]
public class MusicAudioAddressablesProvider : MonoBehaviour, IAddressablesProvider
{
    [SerializeField] private MusicAudioPlayer _musicAudioPlayer;
    
    public void Set()
    {
        MusicAudioAddressablesLoader loader = AddressablesLoaderFactory.Get<MusicAudioAddressablesLoader>();
        _musicAudioPlayer.SetClips(loader.Clips);
    }
}