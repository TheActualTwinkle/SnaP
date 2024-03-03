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

    private void Start()
    {
        Mixer.GetFloat(MusicParameterName, out float value);
        bool active = value <= -80;
        _musicCrossImage.enabled = active;
        Mixer.SetFloat(MusicParameterName, value);
        
        Mixer.GetFloat(SfxParameterName, out value);
        active = value <= -80;
        _sfxCrossImage.enabled = active;
        Mixer.SetFloat(SfxParameterName, value);
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
        bool active = value > -80;

        if (active == true)
        {
            value = -80;
        }
        else
        {
            value = -25;
        }
        
        _musicCrossImage.enabled = active;
        Mixer.SetFloat(MusicParameterName, value);
    }

    // Button
    public void ChangeSfxState()
    {
        Mixer.GetFloat(SfxParameterName, out float value);
        bool active = value > -80;

        if (active == true)
        {
            value = -80;
        }
        else
        {           
            value = 0;
        }
        
        _sfxCrossImage.enabled = active;
        Mixer.SetFloat(SfxParameterName, value);
    }
}
