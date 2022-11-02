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
        public IForce Force { get; }
        public bool IsCombatAvailable { get; }
        public bool IsTpRecoverNeed { get; }
        public bool IsHpRecoverNeed { get; }
        public bool IsReachable { get; }
        public bool IsCombatRange { get; }
        public bool IsDodgeAvailable { get; }
        public bool IsReposition { get; }
        public IDodge Dodge { get; }
        public ICombatForm CombatForm { get; }

        public CombatUnitSummary(CombatUnit unit, CombatUnit target, ICombatForm combatForm, IDodge dodge,
            IForce force, bool isCombatAvailable, bool isDodgeAvailable)
        {
            var status = unit.Status;
            IsSurrenderCondition = unit.IsSurrenderCondition;
            IsTargetSurrenderCondition = target.IsSurrenderCondition;
            IsCombatAvailable = isCombatAvailable;
            CombatForm = combatForm; //获取攻击招式
            Dodge = dodge; //获取身法
            IsCombatRange = unit.IsCombatRange(target);
            IsDodgeAvailable = isDodgeAvailable;
            IsReposition = !IsCombatRange && IsDodgeAvailable;
            IsReachable =  IsCombatRange || IsDodgeAvailable;
            IsHpRecoverNeed = CheckRecoverStrategy(status.Hp);
            IsTpRecoverNeed = CheckRecoverStrategy(status.Tp);
            Force = force;

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


        public CombatPlans GetPlan(CombatManager.Judgment mode)
        {
            //恢复优先
            if (IsHpRecoverNeed) return Force != null ? CombatPlans.RecoverHp : CombatPlans.Wait;
            if (IsTpRecoverNeed) return Force != null ? CombatPlans.RecoverTp : CombatPlans.Wait;
            //逃跑/认输:如果自己达条件，对方没意思逃跑则触发
            if (mode == CombatManager.Judgment.Test &&
                IsSurrenderCondition &&
                !IsTargetSurrenderCondition)
                return CombatPlans.Surrender;
            //攻击
            if (IsCombatAvailable && IsReachable && !IsSurrenderCondition) return CombatPlans.Attack;
            return CombatPlans.Wait;
        }
    }
}