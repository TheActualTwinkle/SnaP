using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GameMainIcon : MonoBehaviour
{
    [SerializeField] private Image _image;
    
    public void SetSprite(Sprite sprite)
    {
        _image.sprite = sprite;
    }
}