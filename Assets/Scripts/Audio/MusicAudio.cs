using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicAudio : MonoBehaviour
{
    private static MusicAudio Instance { get; set; }

    [SerializeField] private AudioSource _audioSource;
    private Dictionary<Constants.Sound.Music.Type, AudioClip> _audioClips = new();

    [SerializeField] private float _musicInterval;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        foreach (KeyValuePair<Constants.Sound.Music.Type, string> keyValuePair in Constants.Sound.Music.Paths)
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

    private void Start()
    {
        if (_audioSource.isPlaying == true)
        {
            _audioSource.Stop();
        }
        
        StartCoroutine(PlayAllClips());
    }

    private IEnumerator PlayAllClips()
    {
        while (true)
        {
            Shuffle(ref _audioClips);
            
            foreach (AudioClip audioClip in _audioClips.Values)
            {
                _audioSource.clip = audioClip;
                _audioSource.Play();

                yield return new WaitUntil(() => _audioSource.isPlaying == false);
                yield return new WaitForSeconds(_musicInterval);
            }
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private static void Shuffle<T1, T2>(ref Dictionary<T1, T2> dictionary)
    {
        System.Random random = new();
        dictionary = dictionary.OrderBy(x => random.Next()).ToDictionary(item => item.Key, item => item.Value); 
    }
}
