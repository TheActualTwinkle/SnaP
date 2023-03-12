using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class InputFiledSliderSetter : MonoBehaviour, ISliderSetter
{
    public float IntervalPerScroll { get; set; }
 
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_InputField _betInputField;

    private void OnEnable()
    {        
        _betInputField.onEndEdit.AddListener(OnBetInputFieldEndEdit);
    }

    private void OnDisable()
    {
        _betInputField.onEndEdit.RemoveListener(OnBetInputFieldEndEdit);
    }

    public void SetValue(Slider slider, float value)
    {
        slider.value = value;
    }

    private void OnBetInputFieldEndEdit(string value)
    {
        if (float.TryParse(value, out float parsedValue) == true)
        {
            SetValue(_slider, parsedValue);
        }
    }
}
