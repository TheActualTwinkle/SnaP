using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public abstract class ViewWindow : MonoBehaviour
{
    public ViewWindow PreviousWindow => _previousWindow;
    [SerializeField] protected ViewWindow _previousWindow;
    
    public async void Show(int delayMs = 0)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
        
        ShowInternal();
        
        gameObject.SetActive(true);
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
        
        gameObject.SetActive(false);
    }

    protected virtual void ShowInternal() {}
    protected virtual void HideInternal() {}
}
