using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace BattleM
{
    public class BattleStage
    {
        private CombatManager CombatManager { get; set; }
        public IEnumerable<CombatUnit> GetCombatUnits() => CombatManager.AllUnits;
        public IEnumerable<CombatUnit> GetAliveUnits() => CombatManager.GetAliveCombatUnits();
        public CombatUnit GetCombatUnit(int combatId) => CombatManager.GetFromAllUnits(combatId);
        public bool IsFightEnd => CombatManager.IsFightEnd;
        public int WinningStance => CombatManager.WinningStance;
        public event UnityAction<int> OnBattleFinalize;
        public event UnityAction<FightRoundRecord> OnEveryRound;
        private CombatRound Round { get; set; }

        public BattleStage(CombatUnit[] units, CombatManager.Judgment judgment,int minEscapeRounds)
        {
            CombatManager = new CombatManager(units, judgment);
            Round = new CombatRound(CombatManager, minEscapeRounds, false);
        }

        public void NextRound(bool autoPlan)
        {
            if (IsFightEnd)
            {
                OnBattleFinalize?.Invoke(WinningStance);
                return;
            }
            var rec = Round.NextRound(autoPlan);
            OnEveryRound?.Invoke(rec);
        }

        public void PrePlan() => Round.CombatPlan(Array.Empty<int>());
    }
}