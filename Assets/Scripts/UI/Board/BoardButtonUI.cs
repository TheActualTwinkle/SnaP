using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BoardButtonUI : MonoBehaviour
{
    private Image _image;
    
    private static PlayerSeatsUI PlayerSeatsUI => PlayerSeatsUI.Instance;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

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
        transform.position = PlayerSeatsUI.Seats[position].transform.position;
    }
}
