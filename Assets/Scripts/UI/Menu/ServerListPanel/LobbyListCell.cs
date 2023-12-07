using System.Collections;
using System.Collections.Generic;
using PolyAndCode.UI;
using SDT;
using TMPro;
using UnityEngine;

public class LobbyListCell : MonoBehaviour, ICell
{
    [SerializeField] private TextMeshProUGUI _indexText;
    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _lobbyPlayersCountText; // e.g "2/5"

    public void SetLobbyInfo(LobbyInfo lobbyInfo, int index)
    {
        _lobbyNameText.text = lobbyInfo.LobbyName;
        _lobbyPlayersCountText.text = $"{lobbyInfo.PlayersCount}/{lobbyInfo.MaxSeats}";

        _indexText.text = index.ToString();
    }
}
