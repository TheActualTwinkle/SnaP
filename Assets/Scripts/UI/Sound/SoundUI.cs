using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundUI : MonoBehaviour
{
    [SerializeField] private Image _musicCrossImage;
    [SerializeField] private Image _sfxCrossImage;

    [SerializeField] private Image _musixButtonImage;
    
    private AudioMixer Mixer => MixerSingleton.Instance.Mixer;

    private string MusicParameterName => MixerSingleton.MusicParameterName;
    private string SfxParameterName => MixerSingleton.SfxParameterName;
    
    private bool MusicEnabled => Mixer.GetFloat(MusicParameterName, out float value) && value >= MusicDefaultVolume;
    private bool SfxEnabled => Mixer.GetFloat(SfxParameterName, out float value) && value >= SfxDefaultVolume;
    
    private const int MusicDefaultVolume = -25;
    private const int SfxDefaultVolume = 0;
    
    private const int MusicMuteVolume = -80;

    private void Start()
    {
        SetupSoundData();
    }

    public void SetSprites(Sprite musicSprite, Sprite crossSprite)
    {
        _musixButtonImage.sprite = musicSprite;
        
        _musicCrossImage.sprite = crossSprite;
        _sfxCrossImage.sprite = crossSprite;
    }

    // Button
    public void ChangeMusicState()
    {
        Mixer.GetFloat(MusicParameterName, out float value);
        bool active = value > MusicMuteVolume;

        if (active == true)
        {
            value = MusicMuteVolume;
        }
        else
        {
            value = MusicDefaultVolume;
        }
        
        _musicCrossImage.enabled = active;
        Mixer.SetFloat(MusicParameterName, value);
    }

    // Button
    public void ChangeSfxState()
    {
        Mixer.GetFloat(SfxParameterName, out float value);
        bool active = value > MusicMuteVolume;

        if (active == true)
        {
            value = MusicMuteVolume;
        }
        else
        {           
            value = SfxDefaultVolume;
        }
        
        _sfxCrossImage.enabled = active;
        Mixer.SetFloat(SfxParameterName, value);
    }

    private void OnDestroy()
    {
        SaveLoadSystemFactory.Instance.Get().Save(new SoundData(MusicEnabled, SfxEnabled));
    }

    private void SetupSoundData()
    {
        if (MusicEnabled == true && SfxEnabled == true)
        {
            return;
        }
        
        SoundData soundData = SaveLoadSystemFactory.Instance.Get().Load<SoundData>();
        Mixer.SetFloat(MusicParameterName, soundData.MusicEnabled ? MusicDefaultVolume : MusicMuteVolume);
        _musicCrossImage.enabled = !soundData.MusicEnabled;

        Mixer.SetFloat(SfxParameterName, soundData.SfxEnabled ? SfxDefaultVolume : MusicMuteVolume);
        _sfxCrossImage.enabled = !soundData.SfxEnabled;
    }
}
