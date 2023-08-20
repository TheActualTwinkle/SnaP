using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BoardButtonUI : MonoBehaviour
{
    [SerializeField] private Image _image;

    [SerializeField] private Vector2 _position;

    private static PlayerSeatsUI PlayerSeatsUI => PlayerSeatsUI.Instance;
    private static BoardButton BoardButton => BoardButton.Instance;
    
    private void OnEnable()
    {
        Game.Instance.EndDealEvent += OnEndDeal;
        BoardButton.PositionNetworkVariable.OnValueChanged += OnBoardButtonPositionChanged;
    }

    private void OnDisable()
    {
        Game.Instance.EndDealEvent -= OnEndDeal;
        BoardButton.PositionNetworkVariable.OnValueChanged -= OnBoardButtonPositionChanged;
    }

    private void Start()
    {
        int buttonPosition = BoardButton.PositionNetworkVariable.Value;
        if (buttonPosition == BoardButton.EmptyPosition)
        {
            return;
        }
        
        Show();
        MoveTo(buttonPosition);
    }

    private void OnBoardButtonPositionChanged(int position, int newValue)
    {
        Show();
        MoveTo(newValue);
    }

    private void OnEndDeal(WinnerInfo[] winnerInfo)
    {
        Hide();
    }

    private void Show()
    {
        _image.enabled = true;
    }

    private void Hide()
    {
        _image.enabled = false;
    }

    private void MoveTo(int position)
    {
        Transform seatTransform = PlayerSeatsUI.Seats[position].transform;
        
        transform.SetParent(seatTransform);
        transform.localPosition = _position;
    }
}