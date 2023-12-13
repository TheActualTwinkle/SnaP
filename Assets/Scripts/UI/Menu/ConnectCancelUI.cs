using System;
using System.Collections;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Animator), typeof(Button))]
public class ConnectCancelUI : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TextMeshProUGUI _connectText;
    [SerializeField] private Image _fadeImage;

    [SerializeField] private float _cancelButtonDisabledTime;
    
    private static readonly int Enable = Animator.StringToHash("Enable");
    private static readonly int Disable = Animator.StringToHash("Disable");
    private static readonly int ConnectionFail = Animator.StringToHash("ConnectionFail");

    private IEnumerator _enableCancelButtonAfterTime;

    private void OnEnable()
    {
        if (_createButton != null) _createButton.onClick.AddListener(OnCreateButtonClick);
        if (_joinButton != null) _joinButton.onClick.AddListener(OnJoinButtonClick);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(OnCancelButtonClick);
    }

    private void OnDisable()
    {
        if (_createButton != null) _createButton.onClick.RemoveListener(OnCreateButtonClick);
        if (_joinButton != null)  _joinButton.onClick.RemoveListener(OnJoinButtonClick);
        if (_cancelButton != null) _cancelButton.onClick.RemoveListener(OnCancelButtonClick);
    }

    private void OnCreateButtonClick()
    {
        _cancelButton.interactable = false;
        if (_enableCancelButtonAfterTime != null)
        {
            StopCoroutine(_enableCancelButtonAfterTime);
        }

        _enableCancelButtonAfterTime = EnableCancelButtonAfterTime();
        StartCoroutine(_enableCancelButtonAfterTime);

        NetworkConnectorHandler.ConnectionStateChangedEvent += OnConnectionStateChanged;
        
        _connectText.text = "Creating game...";
        DisableMenuInteraction();
    }
    
    private void OnJoinButtonClick()
    {
        _connectText.text = "Connecting...";
        
        NetworkConnectorHandler.ConnectionStateChangedEvent += OnConnectionStateChanged;

        DisableMenuInteraction();
    }
    
    private void OnCancelButtonClick()
    {
        NetworkConnectorHandler.ShutdownTrigger = true;
        NetworkManager.Singleton.Shutdown();
        EnableMenuInteraction();
    }
    
    private void OnConnectionStateChanged(NetworkConnectorHandler.ConnectionState state)
    {
        switch (state)
        {
            case NetworkConnectorHandler.ConnectionState.Canceled:
                _connectText.text = "Connection canceled";
                break;
            case NetworkConnectorHandler.ConnectionState.Successful:
                // If connect is OK we don't need to do anything. Changing scene!
                break;
            case NetworkConnectorHandler.ConnectionState.Failed:
                _connectText.text = "Connection failed";
                
                _animator.SetTrigger(ConnectionFail);

                if (_fadeImage == null)
                {
                    break;
                }
                
                _fadeImage.enabled = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        NetworkConnectorHandler.ConnectionStateChangedEvent -= OnConnectionStateChanged;
    }

    private IEnumerator EnableCancelButtonAfterTime()
    {
        yield return new WaitForSeconds(_cancelButtonDisabledTime);

        _cancelButton.interactable = true;
    }
    
    private void EnableMenuInteraction()
    {
        _animator.ResetTrigger(Enable);
        _animator.SetTrigger(Disable);

        if (_fadeImage == null)
        {
            return;
        }
        
        _fadeImage.enabled = false;
    }

    private void DisableMenuInteraction()
    {
        _animator.ResetTrigger(Disable);
        _animator.SetTrigger(Enable);
        
        if (_fadeImage == null)
        {
            return;
        }
        
        _fadeImage.enabled = true;
    }
}
