using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Animator), typeof(Button))]
public class ConnectingUI : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TextMeshProUGUI _waitConnectText;
    [SerializeField] private Image _fadeImage;
    
    private static readonly int Enable = Animator.StringToHash("Enable");
    private static readonly int Disable = Animator.StringToHash("Disable");
    private static readonly int ConnectionFail = Animator.StringToHash("ConnectionFail");

    private IEnumerator _connectionFailTimerRoutine;

    private void OnEnable()
    {
        _cancelButton.onClick.AddListener(OnCancelButtonClick);
        _joinButton.onClick.AddListener(OnJoinButtonClick);
    }

    private void OnDisable()
    {
        _cancelButton.onClick.RemoveListener(OnCancelButtonClick);
        _joinButton.onClick.RemoveListener(OnJoinButtonClick);
    }

    private void OnJoinButtonClick()
    {
        if (_connectionFailTimerRoutine != null)
        {
            StopCoroutine(_connectionFailTimerRoutine);
        }

        _connectionFailTimerRoutine = StartTimer();
        StartCoroutine(_connectionFailTimerRoutine);

        _animator.ResetTrigger(Disable);
        _animator.SetTrigger(Enable);
        
        _waitConnectText.text = "Connecting...";

        _fadeImage.enabled = true;
    }
    
    private void OnCancelButtonClick()
    {
        NetworkManager.Singleton.Shutdown();        
        if (_connectionFailTimerRoutine != null)
        {
            StopCoroutine(_connectionFailTimerRoutine);
        }
        
        _animator.ResetTrigger(Enable);
        _animator.SetTrigger(Disable);

        _fadeImage.enabled = false;
    }

    private IEnumerator StartTimer()
    {
        UnityTransport unityTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        float time = (float)unityTransport.MaxConnectAttempts * unityTransport.ConnectTimeoutMS / 1000;
        yield return new WaitForSeconds(time);
        
        _fadeImage.enabled = false;
        _animator.SetTrigger(ConnectionFail);
        _waitConnectText.text = "Connection Failed";
    }
}
