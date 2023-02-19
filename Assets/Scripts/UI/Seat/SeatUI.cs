using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeatUI : MonoBehaviour
{
    public Image PlayerImage => _playerImage;
    [SerializeField] private Image _playerImage;

    public TextMeshProUGUI NickNameText => _nickNameText;
    [SerializeField] private TextMeshProUGUI _nickNameText;

    public GameObject PocketCards => _pocketCards;
    [SerializeField] private GameObject _pocketCards;

    public Image NickNameBackgroundImage => _nickNameBackgroundImage;
    [SerializeField] private Image _nickNameBackgroundImage;
}
