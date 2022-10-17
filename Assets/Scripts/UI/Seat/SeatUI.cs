using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeatUI : MonoBehaviour
{
    public Image PlayerImage => _playerImage;
    [SerializeField] private Image _playerImage;

    public TextMeshProUGUI NickName => _nickName;
    [SerializeField] private TextMeshProUGUI _nickName;

    public PocketCardsUI PocketCards => _pocketCards;
    [SerializeField] private PocketCardsUI _pocketCards;
}
