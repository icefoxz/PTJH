using System;

namespace BattleM
{
    /// <summary>
    /// 战斗单位概要，根据<see cref="CombatUnit"/>总结当前状态以决定可行动的策略
    /// </summary>
    public struct CombatUnitSummary
    {
        public bool IsTargetSurrenderCondition { get; }
        public bool IsSurrenderCondition { get; }
        public IForceForm RecoverForm { get; }
        public bool IsTpRecoverNeed { get; }
        public bool IsHpRecoverNeed { get; }
        public bool IsReachable { get; }
        public IDodgeForm DodgeForm { get; }
        public ICombatForm CombatForm { get; }

        public CombatUnitSummary(CombatUnit unit, CombatUnit target, ICombatForm combatForm, IDodgeForm dodgeForm,
            IForceForm forceForm)
        {
            var status = unit.Status;
            IsSurrenderCondition = unit.IsSurrenderCondition;
            IsTargetSurrenderCondition = target.IsSurrenderCondition;
            CombatForm = combatForm; //获取攻击招式
            DodgeForm = dodgeForm; //获取身法
            IsReachable = unit.IsCombatRange(target) || DodgeForm != null;
            IsHpRecoverNeed = CheckRecoverStrategy(status.Hp);
            IsTpRecoverNeed = CheckRecoverStrategy(status.Tp);
            RecoverForm = forceForm;

            bool CheckRecoverStrategy(IGameCondition gameCondition)
            {
                //回气策略
                switch (unit.Strategy)
                {
                    case CombatUnit.Strategies.Steady when status.Mp.Value > 10 && LowerThan(0.4f, gameCondition):
                    case CombatUnit.Strategies.Hazard when status.Mp.Value > 10 && LowerThan(0.2f, gameCondition):
                    case CombatUnit.Strategies.Defend when status.Mp.Value > 10 && LowerThan(0.8f, gameCondition):
                    case CombatUnit.Strategies.RunAway when status.Mp.Value > 10 && LowerThan(0.8f, gameCondition):
                    case CombatUnit.Strategies.DeathFight when status.Mp.Value > 10 && LowerThan(0.4f, gameCondition):
                        return true;
                    default:
                        return false;
                }
            }

            bool LowerThan(float ratio, IGameCondition con) => con.ValueMaxRatio < ratio;
        }

        public CombatPlans GetPlan()
        {
            //恢复优先
            if (IsHpRecoverNeed) return RecoverForm != null ? CombatPlans.RecoverHp : CombatPlans.Wait;
            if (IsTpRecoverNeed) return RecoverForm != null ? CombatPlans.RecoverTp : CombatPlans.Wait;
            //逃跑/认输:如果自己达条件，对方没意思逃跑则触发
            if (IsSurrenderCondition && !IsTargetSurrenderCondition) return CombatPlans.Surrender;
            //攻击
            if (CombatForm != null && IsReachable && !IsSurrenderCondition) return CombatPlans.Attack;
            return CombatPlans.Wait;
        }
    }
}