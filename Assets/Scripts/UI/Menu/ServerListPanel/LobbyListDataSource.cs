using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyAndCode.UI;
using LobbyService;
using LobbyService.Interfaces;
using LobbyService.TcpIp;
using UnityEngine;
using Zenject;

public class LobbyListDataSource : MonoBehaviour, IRecyclableScrollRectDataSource
{
    public event Action StartLoadingEvent;
    public event Action EndLoadingEvent;
    
    [SerializeField] private RecyclableScrollRect _recyclableScrollRect;
    [SerializeField] private KeyCode _updateRectKeyCode;
    
    private List<LobbyDto> _lobbyInfos = new();

    private IClientsLobbyService _clientsLobbyService;
    
    private CancellationTokenSource _loadCancellationToken;

    [Inject]
    private void Construct(IClientsLobbyService clientsLobbyService)
    {
        _clientsLobbyService = clientsLobbyService;
    }
    
    private void Awake()
    {
#if !UNITY_STANDALONE
        Destroy(gameObject);
        return;
#endif
        
        _recyclableScrollRect.DataSource = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(_updateRectKeyCode) == false)
        {
            return;
        }

        UpdateScrollRect();
    }

    public int GetItemCount()
    {
        return _lobbyInfos.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        LobbyListCell lobbyListCell = cell as LobbyListCell;

        if (lobbyListCell == null)
        {
            Logger.Log("Can`t cast cell to LobbyListCell. WHY?!", Logger.LogLevel.Error);
            return;
        }
        
        lobbyListCell.SetLobbyInfo(_lobbyInfos[index], index);
    }

    public void CancelLoading()
    {
        _loadCancellationToken?.Cancel();
    }
    
    // Button.
    public async void UpdateScrollRect()
    {
        if (await TryUpdateLobbiesInfo() == false)
        {
            return;
        }

        _lobbyInfos = _lobbyInfos.Where(lobbyInfo => lobbyInfo != null).ToList();
        
        _recyclableScrollRect.ReloadData();
        
        Logger.Log("Scroll rect updated.");
    }
    
    private async Task<bool> TryUpdateLobbiesInfo()
    {
        StartLoadingEvent?.Invoke();
        
        _loadCancellationToken = new CancellationTokenSource();
        List<LobbyDto> lobbyInfos;
        try
        {
            lobbyInfos = await _clientsLobbyService.GetLobbiesInfoAsync(_loadCancellationToken.Token);
        }
        catch (TaskCanceledException)
        {
            Logger.Log("Loading canceled.");
            EndLoadingEvent?.Invoke();
            return false;
        }
        
        if (lobbyInfos == null)
        {
            _lobbyInfos = new List<LobbyDto>();
            EndLoadingEvent?.Invoke();
            return false;
        }
        
        EndLoadingEvent?.Invoke();

        _lobbyInfos = lobbyInfos;
        return true;
    }
}
