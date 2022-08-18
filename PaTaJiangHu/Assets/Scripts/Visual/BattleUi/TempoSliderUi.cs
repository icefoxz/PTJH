using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUi;

public class TempoSliderUi : UiBase
{
    [SerializeField] private Slider slider;
    [SerializeField] private Text _nameText;

    public void UpdateSlider(float value) => slider.value = value;
    public void Init(string text)
    {
        ResetSlider();
        _nameText.text = text;
    }

    public override void ResetUi()
    {
        ResetSlider();
        _nameText.text = string.Empty;
    }
    private void ResetSlider() => slider.value = 0;
}