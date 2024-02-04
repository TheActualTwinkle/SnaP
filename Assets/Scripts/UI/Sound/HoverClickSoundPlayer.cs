using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class HoverClickSoundPlayer : EventTrigger  
{
    private AudioSource _audioSource;
    private static AudioClip _hoverAudioClip;
    private static AudioClip _clickAudioClip;

    private Button _button;
    
    protected virtual void Awake()
    {
        // Set up the loader
        SetupLoader();

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

    public static void SetClips(AudioClip hoverAudioClip, AudioClip clickAudioClip)
    {
        _hoverAudioClip = hoverAudioClip;
        _clickAudioClip = clickAudioClip;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        _audioSource.clip = _hoverAudioClip;
        _audioSource.Play();
    }

    private void SetupLoader()
    {
        
        HoverClickSoundPlayerAddressablesLoader loader = gameObject.AddComponent<HoverClickSoundPlayerAddressablesLoader>();
        loader.HoverType = Constants.Sound.Sfx.Type.ButtonHover;
        loader.ClickType = Constants.Sound.Sfx.Type.ButtonClick;
    }

    private void OnButtonClick()
    {
        _audioSource.clip = _clickAudioClip;
        _audioSource.Play();
    }
}
