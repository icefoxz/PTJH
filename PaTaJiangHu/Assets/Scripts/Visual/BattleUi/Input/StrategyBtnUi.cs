using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi.Input
{
    public class StrategyBtnUi : ButtonUi
    {
        [SerializeField] private Outline _outline;
        protected override void AdditionInit(Button button, Text text)
        {
            Show();
            _outline.enabled = false;
        }

        public void SetSelected(bool isSelected) => _outline.enabled = isSelected;
        public override void ResetUi()
        {
            _outline.enabled = false;
            base.ResetUi();
        }
    }
}