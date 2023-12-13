using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerListScrollView : MenuWindow
{
    [SerializeField] private LobbyListDataSource _lobbyList;
    [SerializeField] private KeyCode _hideWindowKeyCode;
    
    private void Update()
    {
        if (Input.GetKeyDown(_hideWindowKeyCode) == false)
        {
            return;
        }
        
        Hide();
    }

    protected override void ShowInternal()
    {
        _lobbyList.UpdateScrollRect();
    }
}
