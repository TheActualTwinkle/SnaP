using UnityEngine.UI;

public interface ISliderSetter
{
    public float IntervalPerScroll { set; get; }
    
    void SetValue(Slider slider, float value);
}
