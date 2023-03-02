using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundUI : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private Image _musicCrossImage;
    [SerializeField] private Image _sfxCrossImage;

    private void Start()
    {
        _mixer.GetFloat("MusicVol", out float value);
        bool active = value <= -80;
        _musicCrossImage.enabled = active;
        _mixer.SetFloat("MusicVol", value);
        
        _mixer.GetFloat("SFXVol", out value);
        active = value <= -80;
        _sfxCrossImage.enabled = active;
        _mixer.SetFloat("SFXVol", value);
    }

    // Button
    private void ChangeMusicState()
    {
        _mixer.GetFloat("MusicVol", out float value);
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
        _mixer.SetFloat("MusicVol", value);
    }

    // Button
    private void ChangeSfxState()
    {
        _mixer.GetFloat("SFXVol", out float value);
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
        _mixer.SetFloat("SFXVol", value);
    }
}
