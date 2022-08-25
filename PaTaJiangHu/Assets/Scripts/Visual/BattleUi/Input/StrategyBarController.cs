using UnityEngine;
using UnityEngine.Events;
using Visual.BaseUi;

namespace Visual.BattleUi.Input
{
    public class StrategyBarController : OptionController<StrategyBtnUi>
    {
        public void Init() => BaseInit(false);
        public void SetSelected(StrategyBtnUi selectedUi)
        {
            foreach (var ui in List) ui.SetSelected(selectedUi == ui);
        }
    }
}