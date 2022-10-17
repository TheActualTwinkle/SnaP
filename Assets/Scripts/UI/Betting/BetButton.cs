using System;
using UnityEngine;
using UnityEngine.UI;

public class BetButton : MonoBehaviour
{
    public event Action<BetButtonActions> OnClickEvent;

    [SerializeField] private Button _button;
    [SerializeField] private BetButtonActions _action;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        OnClickEvent?.Invoke(_action);
    }
}
