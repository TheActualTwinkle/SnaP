using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(SfxAudio))]
public class SfxAudioAddressableContentUser : MonoBehaviour, IAddressableContentUser
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount { get; private set; }

    [SerializeField] private SfxAudio _sfxAudio;
    
    private Dictionary<Constants.Sound.Sfx.Type, AudioClip> _clips = new();

    
    private async void Start()
    {
        await LoadContent();
    }

    private void OnDestroy()
    {
        UnloadContent();
    }
    
    public async Task LoadContent()
    {
        foreach (KeyValuePair<Constants.Sound.Sfx.Type, string> keyValuePair in Constants.Sound.Sfx.Paths)
        {
            AudioClip audioClip = await AddressablesLoader.LoadAsync<AudioClip>(keyValuePair.Value);
            _clips.Add(keyValuePair.Key, audioClip);
        }
        
        _sfxAudio.SetupAudioClips(_clips);
    }

    public void UnloadContent()
    {
        foreach (KeyValuePair<Constants.Sound.Sfx.Type, AudioClip> keyValuePair in _clips)
        {
            AddressablesLoader.Unload(keyValuePair.Value);
        }
    }
}
