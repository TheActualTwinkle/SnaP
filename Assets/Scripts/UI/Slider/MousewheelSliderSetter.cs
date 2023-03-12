using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MousewheelSliderSetter : MonoBehaviour, ISliderSetter
{    
    public float IntervalPerScroll { get; set; }

    private const int SliderMultiplierPerScroll = 10; // Setup by tests.
    
    [SerializeField] private Slider _slider;
    
    private bool _isHovered;
    
    private void Update()
    {
        float wheelDelta = Input.GetAxis("Mouse ScrollWheel");
        if (_isHovered == true && wheelDelta != 0f)
        {
            SetValue(_slider, _slider.value + (wheelDelta * IntervalPerScroll * SliderMultiplierPerScroll));
        }
    }
    
    public void SetValue(Slider slider, float value)
    {
        slider.value = value;
    }

    // Unity event
    private void OnPointerEnter()
    {
        _isHovered = true;
    }

    // Unity event
    private void OnPointerExit()
    {
        _isHovered = false;
    }
}
