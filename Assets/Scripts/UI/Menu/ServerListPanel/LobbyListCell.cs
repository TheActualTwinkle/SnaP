using System.Collections;
using System.Collections.Generic;
using PolyAndCode.UI;
using SDT;
using TMPro;
using UnityEngine;

public class LobbyListCell : MonoBehaviour, ICell
{
    public static LobbyInfo SelectedLobbyInfo { get; private set; }

    [SerializeField] private TextMeshProUGUI _indexText;
    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _lobbyPlayersCountText; // e.g "2/5"
    
    private LobbyInfo _lobbyInfo;

    public void SetLobbyInfo(LobbyInfo lobbyInfo, int index)
    {
        _lobbyInfo = lobbyInfo;
        
        _lobbyNameText.text = lobbyInfo.LobbyName;
        _lobbyPlayersCountText.text = $"{lobbyInfo.PlayersCount}/{lobbyInfo.MaxSeats}";

        _indexText.text = index.ToString();
    }
    
    // Button.
    private void SelectLobby()
    {
        Logger.Log($"Lobby (id: {_indexText.text}) selected.");
        SelectedLobbyInfo = _lobbyInfo;
    } 
}
