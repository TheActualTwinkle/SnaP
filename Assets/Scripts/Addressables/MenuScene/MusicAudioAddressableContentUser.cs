using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MusicAudio))]
public class MusicAudioAddressableContentUser : MonoBehaviour, IAddressableContentUser
{
    public uint LoadedCount { get; private set; }
    public uint AssetsCount => (uint)Constants.Sound.Music.Paths.Count;

    private MusicAudio _musicAudio;
    
    private readonly List<AudioClip> _clips = new();

    private static bool _isLoaded;
    
    private void Awake()
    {
        if (_isLoaded == true)
        {
            DestroyImmediate(this);
            return;
        }
        
        _musicAudio = GetComponent<MusicAudio>();
    }

    private async void Start()
    {
        await LoadContent();
    }

    private void OnApplicationQuit()
    {
        UnloadContent();
    }

    public async Task LoadContent()
    {
        foreach (string path in Constants.Sound.Music.Paths)
        {
            _clips.Add(await AddressablesLoader.LoadAsync<AudioClip>(path));
            LoadedCount++;
        }

        _musicAudio.SetClips(_clips);
        _isLoaded = true;
    }

    public void UnloadContent()
    {
        foreach (AudioClip audioClip in _clips)
        {
            LoadedCount--;
            AddressablesLoader.Unload(audioClip);
        }
    }
}
