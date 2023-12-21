using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ServerListScrollView : MenuWindow
{
    [SerializeField] private LobbyListDataSource _lobbyList;
    [SerializeField] private KeyCode _hideWindowKeyCode;

    private Animator _animator;
    
    private static readonly int Loading = Animator.StringToHash("Loading");
    
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
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        
        _lobbyList.StartLoadingEvent += OnStartLoadingLobbyList;
        _lobbyList.EndLoadingEvent += OnEndLoadingLobbyList;

        _lobbyList.UpdateScrollRect();
    }

    protected override void HideInternal()
    {
        _lobbyList.CancelLoading();
        
        _lobbyList.StartLoadingEvent -= OnStartLoadingLobbyList;
        _lobbyList.EndLoadingEvent -= OnEndLoadingLobbyList;
    }
    
    private void OnStartLoadingLobbyList()
    {
        _animator.SetBool(Loading, true);
    }

    private void OnEndLoadingLobbyList()
    {
        _animator.SetBool(Loading, false);
    }
}
