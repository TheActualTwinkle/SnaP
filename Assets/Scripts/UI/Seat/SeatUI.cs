using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class SeatUI : MonoBehaviour
{
    public Image PlayerImage => _playerImage;
    [SerializeField] private Image _playerImage;

    public TextMeshProUGUI NickName => _nickName;
    [SerializeField] private TextMeshProUGUI _nickName;

    public GameObject PocketCards => _pocketCards;
    [SerializeField] private GameObject _pocketCards;
}
