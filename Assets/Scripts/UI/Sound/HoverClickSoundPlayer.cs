using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class HoverClickSoundPlayer : EventTrigger  
{
    private AudioSource _audioSource;
    private AudioClip _hoverAudioClip;
    private AudioClip _clickAudioClip;

    private Button _button;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        TryGetComponent(out _button);
    }

    private void OnEnable()
    {
        if (_button == null)
        {
            return;
        }
        
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnDisable()
    {   
        if (_button == null)
        {
            return;
        }
        
        _button.onClick.RemoveListener(OnButtonClick);
    }

    private void Start()
    {
        string pathToHoverSfx = Constants.Sound.Sfx.Paths[Constants.Sound.Sfx.Type.ButtonHover];
        _hoverAudioClip = Resources.Load<AudioClip>(pathToHoverSfx);  
        
        string pathToMusicSfx = Constants.Sound.Sfx.Paths[Constants.Sound.Sfx.Type.ButtonClick];
        _clickAudioClip = Resources.Load<AudioClip>(pathToMusicSfx);
        
        if (_hoverAudioClip == null)
        {
            Logger.Log($"Audio Clip named '{pathToHoverSfx}' not found!", Logger.LogLevel.Error);
        }      
        
        if (_clickAudioClip == null)
        {
            Logger.Log($"Audio Clip named '{pathToMusicSfx}' not found!", Logger.LogLevel.Error);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        _audioSource.clip = _hoverAudioClip;
        _audioSource.Play();
    }

    private void OnButtonClick()
    {
        _audioSource.clip = _clickAudioClip;
        _audioSource.Play();
    }
}
