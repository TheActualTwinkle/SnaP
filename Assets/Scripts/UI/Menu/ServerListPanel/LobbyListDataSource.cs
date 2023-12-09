using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolyAndCode.UI;
using SDT;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyListDataSource : MonoBehaviour, IRecyclableScrollRectDataSource
{
    [SerializeField] private RecyclableScrollRect _recyclableScrollRect;
    [SerializeField] private StandaloneClient _sdtStandaloneClient;
    
    private List<LobbyInfo> _lobbyInfos = new();

    private void Awake()
    {
        _recyclableScrollRect.DataSource = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) == false)
        {
            return;
        }

        UpdateScrollRect();
    }

    public int GetItemCount()
    {
        return _lobbyInfos.Count;
    }

    public async void SetCell(ICell cell, int index)
    {
        LobbyListCell lobbyListCell = cell as LobbyListCell;

        if (lobbyListCell == null)
        {
            Logger.Log("Can`t cast cell to LobbyListCell. WHY?!", Logger.LogLevel.Error);
            return;
        }
        
        await TryUpdateLobbiesInfo();
        lobbyListCell.SetLobbyInfo(_lobbyInfos[index], index);
    }

    // Button.
    public async void UpdateScrollRect()
    {
        if (await TryUpdateLobbiesInfo() == false)
        {
            return;
        }
        
        _recyclableScrollRect.ReloadData();
        
        Logger.Log("Scroll rect updated.");
    }
    
    private async Task<bool> TryUpdateLobbiesInfo()
    {
        List<LobbyInfo> lobbyInfos = await _sdtStandaloneClient.GetLobbiesInfoAsync();
        
        if (lobbyInfos == null)
        {
            _lobbyInfos = new List<LobbyInfo>();
            return false;
        }
        
        _lobbyInfos = lobbyInfos;
        return true;
    }
}
