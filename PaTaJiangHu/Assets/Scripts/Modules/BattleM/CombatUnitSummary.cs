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
        public bool IsCombatAvailable { get; }
        public bool IsHpRecoverNeed { get; }
        public bool IsReachable { get; }
        public bool IsCombatRange { get; }
        public bool IsDodgeAvailable { get; }
        public bool IsReposition { get; }
        public IForceSkill Force { get; }
        public IDodgeSkill Dodge { get; }
        public ICombatForm CombatForm { get; }

        public CombatUnitSummary(CombatUnit unit, ICombatInfo target, ICombatForm combatForm, IDodgeSkill dodge,
            IForceSkill force, bool isCombatAvailable, bool isDodgeAvailable)
        {
            var status = unit.Status;
            Force = force;
            Dodge = dodge;
            CombatForm = combatForm;
            IsSurrenderCondition = unit.IsSurrenderCondition;
            IsTargetSurrenderCondition = target.IsSurrenderCondition;
            IsCombatAvailable = isCombatAvailable;
            IsCombatRange = unit.IsCombatRange(target);
            IsDodgeAvailable = isDodgeAvailable;
            IsReposition = !IsCombatRange && IsDodgeAvailable;
            IsReachable =  IsCombatRange || IsDodgeAvailable;
            IsHpRecoverNeed = CheckRecoverStrategy(status.Hp);

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

        public IPerform GetAutoPerform() => new AutoPerform(Force, Dodge, CombatForm, Force, IsReposition);

        public CombatEvents GetPlan(CombatUnitManager.Judgment mode)
        {
            //恢复优先
            if (IsHpRecoverNeed) return Force != null ? CombatEvents.Recover : CombatEvents.Wait;
            //逃跑/认输:如果自己达条件，对方没意思逃跑则触发
            if (mode == CombatUnitManager.Judgment.Test &&
                IsSurrenderCondition &&
                !IsTargetSurrenderCondition)
                return CombatEvents.Surrender;
            //攻击
            if (IsCombatAvailable && IsReachable && !IsSurrenderCondition) return CombatEvents.Offend;
            return CombatEvents.Wait;
        }

        private record AutoPerform(IForceSkill ForceSkill, IDodgeSkill DodgeSkill, ICombatForm CombatForm, IRecovery Recover, bool IsReposition) : IPerform
        {
            public IForceSkill ForceSkill { get; } = ForceSkill;
            public IDodgeSkill DodgeSkill { get; } = DodgeSkill;
            public IPerform.Activities Activity { get; } = IPerform.Activities.Auto;
            public ICombatForm CombatForm { get; } = CombatForm;
            public IRecovery Recover { get; } = Recover;
            public bool IsReposition { get; } = IsReposition;
        }
    }
}