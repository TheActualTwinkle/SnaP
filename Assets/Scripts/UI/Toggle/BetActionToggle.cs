using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetActionToggle : MonoBehaviour
{
    public Toggle Toggle => _toggle;
    [SerializeField] private Toggle _toggle;

    public BetAction BetAction => _betAction;
    [SerializeField] private BetAction _betAction;
    
    [SerializeField] private TextMeshProUGUI _text;
    
    public void SetToggleInfo(BetAction betAction, string toggleText)
    {
        _betAction = betAction;
        _text.text = toggleText;
    }
}