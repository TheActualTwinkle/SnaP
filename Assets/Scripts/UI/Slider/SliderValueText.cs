using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(TextMeshProUGUI))]
public class SliderValueText : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        OnSliderValueChanged(_slider.value);
    }

    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDisable()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        _text.text = value.ToString(CultureInfo.InvariantCulture);
    }
}
