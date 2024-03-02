using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }
    
    [SerializeField] private Canvas _canvas;
    
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _loadingInfo;

    [SerializeField] private float _barSpeed;
    [SerializeField] private float _loadEndDelaySeconds;

    private const float MaxTargetProgress = 1f;
    
    private float _targetProgress;
    private bool _isProgress;

    private void Awake()
    {
#if UNITY_SERVER
        Destroy(gameObject);
        return;
#endif
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void Load(Queue<ILoadingOperation> loadingOperations)
    {
        _canvas.enabled = true;
        StartCoroutine(UpdateProgressBar());

        foreach (ILoadingOperation loadingOperation in loadingOperations)
        {
            ResetProgressBar();
            _loadingInfo.text = loadingOperation.Description;

            await loadingOperation.Load(OnProgress);
            await WaitForBarFill();
        }

        _canvas.enabled = false;
    }
    
    private void ResetProgressBar()
    {
        _progressBar.value = 0;
        _targetProgress = 0;
    }

    private void OnProgress(float progress)
    {
        _targetProgress = Mathf.Clamp(progress, 0.0f, MaxTargetProgress);
    }
    
    private async Task WaitForBarFill()
    {
        while (_progressBar.value < _targetProgress)
        {
            await Task.Delay(1);
        }

        await Task.Delay(TimeSpan.FromSeconds(_loadEndDelaySeconds));
    }
    
    private IEnumerator UpdateProgressBar()
    {
        while (_canvas.enabled)
        {
            if (_progressBar.value < _targetProgress)
            {
                _progressBar.value += Time.deltaTime * _barSpeed;
            }

            yield return null;
        }
    }
}
