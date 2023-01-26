using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeatUI : MonoBehaviour
{
    public Image PlayerImage => _playerImage;
    [SerializeField] private Image _playerImage;

    public TextMeshProUGUI NickName => _nickName;
    [SerializeField] private TextMeshProUGUI _nickName;

    public GameObject PocketCards => _pocketCards;
    [SerializeField] private GameObject _pocketCards;

    public Image NickNameBackgroundImage => _nickNameBackgroundImage;
    [SerializeField] private Image _nickNameBackgroundImage;
}
