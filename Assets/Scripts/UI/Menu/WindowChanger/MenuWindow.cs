using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class MenuWindow : MonoBehaviour
{
    public MenuWindow PreviousWindow => _previousWindow;
    [SerializeField] protected MenuWindow _previousWindow;
    
    [SerializeField] protected Button _backButton;

    // Button.
    public void BackButtonClick()
    {
        Hide();
        _previousWindow.Show();
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(true);
    }
}
