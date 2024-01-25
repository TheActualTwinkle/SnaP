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
    private AudioClip _hoverAudioClip;
    private AudioClip _clickAudioClip;

    private Button _button;

    private HoverClickSoundPlayerAddressableContentUser _loader;

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
        LoadClips();
    }

    public void SetClips(AudioClip hoverAudioClip, AudioClip clickAudioClip)
    {
        _hoverAudioClip = hoverAudioClip;
        _clickAudioClip = clickAudioClip;
    }
    
    private async void LoadClips()
    {
        _loader = new HoverClickSoundPlayerAddressableContentUser(this);
        await _loader.LoadContent();
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
