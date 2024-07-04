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
        _audioSource = GetComponent<AudioSource>();

        TryGetComponent(out _button);

        if (_hoverAudioClip == null)
        {
            _hoverAudioClip = Resources.Load<AudioClip>(Constants.Sound.Sfx.Hover);
        }
        
        if (_clickAudioClip == null)
        {
            _clickAudioClip = Resources.Load<AudioClip>(Constants.Sound.Sfx.Click);
        }
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
        _audioSource.outputAudioMixerGroup = MixerSingleton.Instance.SfxGroup;
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
