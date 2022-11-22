using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetActionToggle : MonoBehaviour
{
    public event Action<BetAction> ToggleOnEvent;

    private BetAction _betAction;

    [SerializeField] private Toggle _toggle;
    [SerializeField] private TextMeshProUGUI _text;

    private void Awake()
    {
        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    public void SetToggleInfo(BetAction betAction, string toggleText)
    {
        _betAction = betAction;
        _text.text = toggleText;
    }
    
    private void OnValueChanged(bool value)
    {
        if (value == true)
        {
            ToggleOnEvent?.Invoke(_betAction);
        }
    }
}