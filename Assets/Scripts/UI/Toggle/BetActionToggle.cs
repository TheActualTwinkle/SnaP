using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BetActionToggle : MonoBehaviour, IToggle<BetAction>
{
    public event Action<BetAction> ToggleOnEvent;

    public BetAction EventArgument { get; set; }

    [SerializeField] private Toggle _toggle;

    private void Awake()
    {
        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(bool value)
    {
        if (value == true)
        {
            ToggleOnEvent?.Invoke(EventArgument);
        }
    }
}
