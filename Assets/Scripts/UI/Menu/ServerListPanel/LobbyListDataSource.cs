using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyAndCode.UI;
using SDT;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyListDataSource : MonoBehaviour, IRecyclableScrollRectDataSource
{
    public event Action StartLoadingEvent;
    public event Action EndLoadingEvent;
    
    [SerializeField] private RecyclableScrollRect _recyclableScrollRect;
    [SerializeField] private Client _sdtClient;
    [SerializeField] private KeyCode _updateRectKeyCode;
    
    private List<LobbyInfo> _lobbyInfos = new();

    private CancellationTokenSource _loadCancellationToken;

    private void Awake()
    {
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
        List<LobbyInfo> lobbyInfos;
        try
        {
            lobbyInfos = await _sdtClient.GetLobbiesInfoAsync(_loadCancellationToken);
        }
        catch (TaskCanceledException)
        {
            Logger.Log("Loading canceled.");
            return false;
        }
        
        if (lobbyInfos == null)
        {
            _lobbyInfos = new List<LobbyInfo>();
            return false;
        }
        
        EndLoadingEvent?.Invoke();

        _lobbyInfos = lobbyInfos;
        return true;
    }
}
