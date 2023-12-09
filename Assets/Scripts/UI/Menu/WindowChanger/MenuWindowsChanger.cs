using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuWindowsChanger : MonoBehaviour
{
    private MenuWindow _currentWindow;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == false)
        {
            return;
        }

        if (_currentWindow == null)
        {
            return;
        }

        if (_currentWindow.PreviousWindow == null)
        {
            return;
        }
        
        _currentWindow.Hide();
        _currentWindow = _currentWindow.PreviousWindow;
        _currentWindow.Show();
    }
}
