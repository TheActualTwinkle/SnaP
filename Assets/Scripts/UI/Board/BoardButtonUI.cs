using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BoardButtonUI : MonoBehaviour
{
    [SerializeField] private Image _image;

    [SerializeField] private Vector2 _position;
    
    private static PlayerSeatsUI PlayerSeatsUI => PlayerSeatsUI.Instance;

    private void OnEnable()
    {
        BoardButton.OnMove += OnButtonMove;
    }

    private void OnDisable()
    {
        BoardButton.OnMove -= OnButtonMove;
    }

    private void OnButtonMove(int position)
    {
        _image.enabled = true;

        Transform seatTransform = PlayerSeatsUI.Seats[position].transform;
        
        transform.SetParent(seatTransform);
        transform.localPosition = _position;
    }
}