using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public abstract class MenuWindow : MonoBehaviour
{
    public MenuWindow PreviousWindow => _previousWindow;
    [SerializeField] protected MenuWindow _previousWindow;
    
    // Button.
    public async void BackButtonClick(float delay = 0.0f)
    {
        await Task.Delay((int)(delay * 1000));

        Hide();
        _previousWindow.Show();
    }
    
    public void Show()
    {
        ShowInternal();
        
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        HideInternal();
        
        gameObject.SetActive(false);
    }

    protected virtual void ShowInternal() {}
    protected virtual void HideInternal() {}
}
