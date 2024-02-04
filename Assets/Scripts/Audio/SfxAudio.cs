using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxAudio : MonoBehaviour
{
    public static SfxAudio Instance { get; private set; }

    [SerializeField] private AudioSource _audioSource;
    private Dictionary<Constants.Sound.Sfx.Type, AudioClip> _audioClips = new();

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
    }

    public void SetupAudioClips(Dictionary<Constants.Sound.Sfx.Type, AudioClip> audioClips)
    {
        _audioClips = audioClips;
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
}
