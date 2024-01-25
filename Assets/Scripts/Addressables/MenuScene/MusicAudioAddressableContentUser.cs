using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MusicAudio))]
public class MusicAudioAddressableContentUser : MonoBehaviour, IAddressableContentUser
{
    private MusicAudio _musicAudio;
    
    private readonly List<AudioClip> _clips = new();
    
    private void Awake()
    {
        _musicAudio = GetComponent<MusicAudio>();
    }

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
        foreach (string path in Constants.Sound.Music.Paths)
        {
            _clips.Add(await AddressablesLoader.LoadAsync<AudioClip>(path));
        }

        _musicAudio.SetClips(_clips);
    }

    public void UnloadContent()
    {
        foreach (AudioClip audioClip in _clips)
        {
            AddressablesLoader.Unload(audioClip);
        }
    }
}
