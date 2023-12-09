using System;
using System.Collections;
using System.Collections.Generic;
using SDT;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(SdtConnectionResultHoverTooltip))]
public class SdtConnectionResultUI : MonoBehaviour
{
    private StandaloneClient SdtStandaloneClient => StandaloneClient.Instance;

    [SerializeField] private Image _image;

    [SerializeField] private Sprite _loadingSprite;
    [SerializeField] private Sprite _successSprite;
    [SerializeField] private Sprite _warningSprite;
    [SerializeField] private Sprite _failSprite;
    
    private SdtConnectionResultHoverTooltip _hoverTooltip;

    private Animator _animator;
    
    private static readonly int Loading = Animator.StringToHash("Loading");

    private void Awake()
    {
        _hoverTooltip = GetComponent<SdtConnectionResultHoverTooltip>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        SdtStandaloneClient.ConnectionStateChangedEvent += OnSdtConnectionStateChanged;
    }

    private void OnDisable()
    {
        SdtStandaloneClient.ConnectionStateChangedEvent -= OnSdtConnectionStateChanged;
    }

    private void OnSdtConnectionStateChanged(ConnectionState connectionState)
    {
        switch (connectionState)
        {
            case ConnectionState.Connecting:
                _image.sprite = _loadingSprite;
                _animator.SetBool(Loading, true);
                break;
            case ConnectionState.Successful:
                _image.sprite = _successSprite;
                _animator.SetBool(Loading, false);
                break;
            case ConnectionState.Failed:
                _image.sprite = _failSprite;
                _animator.SetBool(Loading, false);
                break;
            case ConnectionState.Disconnected:
                _image.sprite = _warningSprite;
                _animator.SetBool(Loading, false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, null);
        }
        
        _hoverTooltip.SetupText();
    }

    // Button.
    public void Reconnect()
    {
        if (SdtStandaloneClient.ConnectionState is ConnectionState.Successful or ConnectionState.Connecting)
        {
            return;
        }
        
        SdtStandaloneClient.TryConnect();
    } 
}
