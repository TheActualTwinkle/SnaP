using System;
using TMPro;
using UnityEngine;

public class PopupObject : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    
    [SerializeField] private TMP_Text _text;

    public float TimeOfLife => _timeOfLife;
    [SerializeField] private float _timeOfLife;

    private void Awake()
    {
        _animator.speed = 1 / _timeOfLife;
    }

    public void SetText(string text)
    {
        _text.text = text;
    }
}
