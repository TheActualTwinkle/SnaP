using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TutorialViewWindow : ViewWindow
{
    [SerializeField] private KeyCode _hideWindowKeyCode;
    
    private Animator _animator;
    private static readonly int ShowParameterHash = Animator.StringToHash("Show");

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(_hideWindowKeyCode) == false)
        {
            return;
        }
        
        Hide();
    }
    
    protected override void ShowInternal()
    {
        _animator.SetBool(ShowParameterHash, true);
    }
    
    protected override void HideInternal()
    {
        _animator.SetBool(ShowParameterHash, false);
    }
}