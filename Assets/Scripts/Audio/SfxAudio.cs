using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxAudio : MonoBehaviour
{
    public static SfxAudio Instance { get; private set; }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private List<AudioClip> _audioClips;

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

    // Animator
    public void Play(int audioClipId)
    {
        if (audioClipId >= _audioClips.Count)
        {
            return;
        }

        if (_audioSource.isPlaying == true)
        {
            _audioSource.Stop();
        }
        
        _audioSource.clip = _audioClips[audioClipId];
        _audioSource.Play();
    }
}
