using BattleM;
using Systems.Utls;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi
{
    public class ConValueSliderUi : UiBase
    {
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private RectTransform _maxConRect;

        public void Set(IConditionValue con)
        {
            _hpSlider.value = 1f * con.Value / con.Fix;
            _maxConRect.SetRight(1f * con.Max / con.Fix * _maxConRect.rect.xMax);
        }
        public override void ResetUi()
        {
            _maxConRect.SetRight(0);
            _hpSlider.value = 1;
        }
    }
}