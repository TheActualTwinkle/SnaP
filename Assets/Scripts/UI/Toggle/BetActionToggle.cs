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
    
    private void OnEnable()
    {
        Game.Instance.GameStageBeganEvent += OnGameStageBegan;
    }

    private void OnDisable()
    {
        Game.Instance.GameStageBeganEvent -= OnGameStageBegan;
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

    private void OnGameStageBegan(GameStage gameStage)
    {
        _toggle.isOn = false;
    }
}