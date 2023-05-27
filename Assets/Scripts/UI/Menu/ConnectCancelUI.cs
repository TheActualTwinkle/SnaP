using System.Collections;
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

    private IEnumerator _connectionFailTimerRoutine;
    private IEnumerator _enableCancelButtonAfterTime;

    private void OnEnable()
    {
        _createButton.onClick.AddListener(OnCreateButtonClick);
        _joinButton.onClick.AddListener(OnJoinButtonClick);
        _cancelButton.onClick.AddListener(OnCancelButtonClick);
    }

    private void OnDisable()
    {
        _createButton.onClick.RemoveListener(OnCreateButtonClick);
        _joinButton.onClick.RemoveListener(OnJoinButtonClick);
        _cancelButton.onClick.RemoveListener(OnCancelButtonClick);
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
        
        _connectText.text = "Creating game...";
        DisableMenuInteraction();
    }
    
    private void OnJoinButtonClick()
    {
        _connectText.text = "Connecting...";
        DisableMenuInteraction();
    }
    
    private void OnCancelButtonClick()
    {
        NetworkManager.Singleton.Shutdown();
        EnableMenuInteraction();
    }

    private IEnumerator StartTimer()
    {
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        float time = (float)unityTransport.MaxConnectAttempts * unityTransport.ConnectTimeoutMS / 1000;

        yield return new WaitForSeconds(time);
        
        _fadeImage.enabled = false;
        _animator.SetTrigger(ConnectionFail);
        _connectText.text = "Connect time out";
    }

    private IEnumerator EnableCancelButtonAfterTime()
    {
        yield return new WaitForSeconds(_cancelButtonDisabledTime);

        _cancelButton.interactable = true;
    }
    
    private void EnableMenuInteraction()
    {   
        if (_connectionFailTimerRoutine != null)
        {
            StopCoroutine(_connectionFailTimerRoutine);
        }
        
        _animator.ResetTrigger(Enable);
        _animator.SetTrigger(Disable);

        _fadeImage.enabled = false;
    }

    private void DisableMenuInteraction()
    {
        if (_connectionFailTimerRoutine != null)
        {
            StopCoroutine(_connectionFailTimerRoutine);
        }

        _connectionFailTimerRoutine = StartTimer();
        StartCoroutine(_connectionFailTimerRoutine);

        _animator.ResetTrigger(Disable);
        _animator.SetTrigger(Enable);

        _fadeImage.enabled = true;
    }
}
