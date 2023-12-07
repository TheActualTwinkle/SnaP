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

    private async void Awake()
    {
        _recyclableScrollRect.DataSource = this;
    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) == false)
        {
            return;
        }

        await UpdateScrollRect();
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
        
        await UpdateLobbyInfo();
        lobbyListCell.SetLobbyInfo(_lobbyInfos[index], index);
    }

    // Button.
    private async Task UpdateScrollRect()
    {
        await UpdateLobbyInfo();
        _recyclableScrollRect.ReloadData();
        
        Logger.Log("Scroll rect updated.");
    }
    
    private async Task UpdateLobbyInfo()
    {
        List<LobbyInfo> lobbyInfos = (await _sdtStandaloneClient.GetLobbyInfoAsync()).ToList();
        _lobbyInfos = lobbyInfos;
    }
}
