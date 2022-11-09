using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace BattleM
{
    public class BattleStage
    {
        private CombatUnitManager CombatUnitManager { get; set; }
        public IEnumerable<CombatUnit> GetCombatUnits() => CombatUnitManager.AllUnits;
        public IEnumerable<CombatUnit> GetAliveUnits() => CombatUnitManager.GetAliveCombatUnits();
        public CombatUnit GetCombatUnit(int combatId) => CombatUnitManager.GetFromAllUnits(combatId);
        public bool IsFightEnd => CombatUnitManager.IsFightEnd;
        public int WinningStance => CombatUnitManager.WinningStance;
        public event UnityAction<CombatRoundRecord> OnEveryRound;
        private ICombatRound Round { get; set; }

        public BattleStage(CombatUnit[] units, CombatUnitManager.Judgment judgment,int minEscapeRounds)
        {
            CombatUnitManager = new CombatUnitManager(units, judgment);
            Round = new CombatRound(CombatUnitManager, minEscapeRounds, false);
        }

        public void NextRound(bool autoPlan)
        {
            if (IsFightEnd) return;
            var rec = Round.NextRound(autoPlan);
            OnEveryRound?.Invoke(rec);
        }

        public void PrePlan() => Round.CombatPlan(Array.Empty<int>());
    }
}