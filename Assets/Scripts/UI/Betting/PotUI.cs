using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotUI : MonoBehaviour
{
    [SerializeField] private Image _chipsImage;
    [SerializeField] private TextMeshProUGUI _valueText;

    private static Pot Pot => Pot.Instance;
    
    private void OnEnable()
    {
        Pot.ValueNetworkVariable.OnValueChanged += OnPotValueNetworkVariableValueChanged;
    }

    private void OnDisable()
    {
        Pot.ValueNetworkVariable.OnValueChanged -= OnPotValueNetworkVariableValueChanged;
    }

    private void Start()
    {
        uint potValue = Pot.ValueNetworkVariable.Value;
        if (potValue == 0)
        {
            return;
        }
        
        Show(potValue);
    }

    private void OnPotValueNetworkVariableValueChanged(uint previousValue, uint newValue)
    {
        if (newValue == 0)
        {
            Hide();
            return;
        }
        
        if (_valueText.text != newValue.ToString())
        {
            SfxAudio.Instance.Play(2);
        }
        
        Show(newValue);
    }

    private void Show(uint value)
    {
        _chipsImage.enabled = true;
        _valueText.text = value.ToString();
    }
    
    private void Hide()
    {
        _valueText.text = string.Empty;
        _chipsImage.enabled = false;
    }
}