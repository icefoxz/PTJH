using System.Collections.Generic;
using System.Linq;
using BattleM;
using Visual.BaseUi;
using Visual.BattleUi.Scene;

namespace Visual.BattleUi.Status
{
    public class BattleStanceUi : PrefabController<CombatUnitUi>
    {
        public IEnumerable<CombatUnitUi> CombatUnitUis => List;
        public IEnumerable<int> HandledIds => List.Select(c => c.CombatId);
        public void Init()
        {
            BaseInit(false);
        }
        public void PopText(int combatId, string text) => GetCombatUnitUi(combatId).Pop(text);
        public void PopStatus(int combatId, int hpValue, int tpValue, int mpValue)
        {
            GetCombatUnitUi(combatId).PopHp(hpValue);
            GetCombatUnitUi(combatId).PopTp(tpValue);
            GetCombatUnitUi(combatId).PopMp(mpValue);
        }
        public void SetAction(int combatId, Stickman.Anims action) => GetCombatUnitUi(combatId).SetAnim(action);
        private CombatUnitUi GetCombatUnitUi(int combatId) => List.Single(c => c.CombatId == combatId);
        public Stickman GetStickman(int combatId) => GetCombatUnitUi(combatId).Stickman;

        public void AddCombatUnit(CombatUnit unit)
        {
            AddUi(ui => ui.Set(unit));
        }
        public override void ResetUi()
        {
            foreach (var ui in List.ToArray())
            {
                ui.ResetUi();
                Destroy(ui.gameObject);
            }
            RemoveList();
        }
    }
}