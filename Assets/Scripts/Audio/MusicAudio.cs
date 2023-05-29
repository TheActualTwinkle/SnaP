using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicAudio : MonoBehaviour
{
    private static MusicAudio Instance { get; set; }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private List<AudioClip> _audioClips;
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
            Shuffle(_audioClips);
            
            foreach (AudioClip audioClip in _audioClips)
            {

                _audioSource.clip = audioClip;
                _audioSource.Play();

                yield return new WaitUntil(() => _audioSource.isPlaying == false);
                yield return new WaitForSeconds(_musicInterval);
            }
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private static void Shuffle<T>(IList<T> list)
    {
        System.Random random = new();
        
        int length = list.Count;  
        while (length > 1) {  
            length--;  
            int k = random.Next(length + 1);  
            (list[k], list[length]) = (list[length], list[k]);
        }  
    }
}
