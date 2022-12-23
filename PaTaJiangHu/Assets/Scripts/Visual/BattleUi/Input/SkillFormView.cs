using System;
using Visual.BaseUis;

namespace Visual.BattleUi.Input
{
    public class SkillFormView : PrefabController<SkillFormUi>
    {
        public void Init()
        {
            BaseInit();
        }

        public void AddOption(Action<SkillFormUi> initAction) => AddUi(initAction);

        public override void ResetUi()
        {
            Hide();
            RemoveList();
        }

        public void SetSelected(SkillFormUi ui)
        {
            foreach (var formUi in List) formUi.SetSelected(formUi == ui);
        }
    }
}