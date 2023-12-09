using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerListScrollView : MenuWindow
{
    [SerializeField] private LobbyListDataSource _lobbyList;
    
    protected override void ShowInternal()
    {
        _lobbyList.UpdateScrollRect();
    }
}
