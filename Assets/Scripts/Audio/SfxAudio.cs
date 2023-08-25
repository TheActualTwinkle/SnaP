using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxAudio : MonoBehaviour
{
    public static SfxAudio Instance { get; private set; }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Dictionary<Constants.Sound.Sfx.Type, AudioClip> _audioClips = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        SetupAudioClips();
    }

    public void Play(Constants.Sound.Sfx.Type audioClipType)
    {
        if (_audioSource.isPlaying == true)
        {
            _audioSource.Stop();
        }
        
        _audioSource.clip = _audioClips[audioClipType];
        _audioSource.Play();
    }
    
    private void SetupAudioClips()
    {
        foreach (KeyValuePair<Constants.Sound.Sfx.Type, string> keyValuePair in Constants.Sound.Sfx.Paths)
        {
            AudioClip audioClip = Resources.Load<AudioClip>(keyValuePair.Value);

            if (audioClip == null)
            {
                Log.WriteToFile($"Error: Audio Clip named '{keyValuePair.Value}' not found!");
                continue;
            }
            
            _audioClips.Add(keyValuePair.Key, audioClip);
        }
    }
}
