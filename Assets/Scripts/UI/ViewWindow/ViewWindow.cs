using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public abstract class ViewWindow : MonoBehaviour
{
    public ViewWindow PreviousWindow => _previousWindow;
    [SerializeField] protected ViewWindow _previousWindow;

    private CanvasGroup _canvasGroup;
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public async void Show(int delayMs = 0)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
        
        ShowInternal();

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
    
    public async void ShowPrevious(int delayMs = 0)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
        
        if (_previousWindow == null)
        {
            return;
        }
        
        _previousWindow.Show();
    }

    public async void Hide(int delayMs = 0)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
        
        HideInternal();
        
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    protected virtual void ShowInternal() {}
    protected virtual void HideInternal() {}
}
