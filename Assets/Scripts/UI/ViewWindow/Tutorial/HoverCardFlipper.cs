using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger), typeof(Animator))]
public class HoverCardFlipper : MonoBehaviour
{
    [SerializeField] private Sprite _originalSprite;
    [SerializeField] private Sprite _flippedSprite;

    [SerializeField] private Image _image;
    private Animator _animator;
    
    private static readonly int FlipParameterHash = Animator.StringToHash("Flip");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnHoverEnter()
    {
        _animator.SetBool(FlipParameterHash, true);
    }
    
    public void OnHoverExit()
    {
        _animator.SetBool(FlipParameterHash, false);
    }
    
    private void Flip()
    {
        _image.sprite = _flippedSprite;
    }

    private void FlipBack()
    {
        _image.sprite = _originalSprite;
    }
}
