using UnityEngine;
using UnityEngine.Audio;

public class MixerSingleton : MonoBehaviour
{
    public static MixerSingleton Instance { get; private set; }
    
    public static string MusicParameterName => "MusicVol";
    public static string SfxParameterName => "SFXVol";
    
    public AudioMixer Mixer => _mixer;
    [SerializeField] private AudioMixer _mixer;
    
    public AudioMixerGroup MusicGroup => _musicGroup;
    [SerializeField] private AudioMixerGroup _musicGroup;
    
    public AudioMixerGroup SfxGroup => _sfxGroup;
    [SerializeField] private AudioMixerGroup _sfxGroup;

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
}
