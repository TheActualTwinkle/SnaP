using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicAudio : MonoBehaviour
{
    private static MusicAudio Instance { get; set; }

    [SerializeField] private AudioSource _audioSource;
    private List<AudioClip> _audioClips;

    [SerializeField] private float _musicInterval;

    private bool _isClipsReady;
    
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
        yield return new WaitWhile(() => _audioClips == null);
        
        while (true)
        {
            Shuffle(ref _audioClips);
            
            foreach (AudioClip audioClip in _audioClips)
            {
                _audioSource.clip = audioClip;
                _audioSource.Play();

                yield return new WaitForSeconds(audioClip.length);
                yield return new WaitUntil(() => _audioSource.isPlaying == false);
                yield return new WaitForSeconds(_musicInterval);
            }
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public void SetClips(IEnumerable<AudioClip> clips)
    {
        _audioClips = clips.ToList();
    }

    private static void Shuffle<T>(ref List<T> list)
    {
        System.Random random = new();
        list = list.OrderBy(_ => random.Next()).ToList(); 
    }
}
