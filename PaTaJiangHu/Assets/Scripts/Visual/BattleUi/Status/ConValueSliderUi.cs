using BattleM;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUis;

namespace Visual.BattleUi.Status
{
    public class ConValueSliderUi : BaseUi
    {
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private Slider _maxSlider;

        public void Set(IConditionValue con)
        {
            SetValue(con.Value, con.Max);
            SetSlider(_maxSlider, con.Max, con.Fix);
        }

        public void SetValue(int value, int max)
        {
            SetSlider(_hpSlider, value, max);
        }
        public static void SetSlider(Slider slider,int value,int max)
        {
            slider.value = 1f * value / max;
        }
        public override void ResetUi()
        {
            _maxSlider.value = 1f;
            _hpSlider.value = 1;
        }
    }
}