using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class SeatUI : MonoBehaviour
{
    public Image PlayerImage => _playerImage;
    [SerializeField] private Image _playerImage;
    [SerializeField] private Sprite _avatarLoadingSprite;

    public TextMeshProUGUI NickNameText => _nickNameText;
    [SerializeField] private TextMeshProUGUI _nickNameText;

    public GameObject PocketCards => _pocketCards;
    [SerializeField] private GameObject _pocketCards;

    public Image NickNameBackgroundImage => _nickNameBackgroundImage;
    [SerializeField] private Image _nickNameBackgroundImage;

    [SerializeField] private Animator _animator;
    
    private static readonly int LoadingAvatar = Animator.StringToHash("LoadingAvatar");
    private static readonly int Empty = Animator.StringToHash("Empty");

    public void EnableLoadingImage()
    {
        _playerImage.sprite = _avatarLoadingSprite;
        
        _animator.ResetAllTriggers();
        _animator.SetTrigger(LoadingAvatar);
    }

    public void DisableLoadingImage()
    {
        _playerImage.sprite = null;
        
        _animator.ResetAllTriggers();
        _animator.SetTrigger(Empty);
    }
}
