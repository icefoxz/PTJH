using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Configs.Battles
{
    /// <summary>
    /// 战斗技能(武学)的实例
    /// </summary>
    internal record CombatSet : ICombatSet
    {
        protected virtual List<Func<CombatArgs, float>> HardRate { get; }
        protected virtual List<Func<CombatArgs, float>> HardDamageRatio { get; }
        protected virtual List<Func<CombatArgs, float>> CriticalRate { get; }
        protected virtual List<Func<CombatArgs, float>> CriticalDamageRatio { get; }
        protected virtual List<Func<CombatArgs, float>> MpDamage { get; }
        protected virtual List<Func<CombatArgs, float>> MpCounteract { get; }
        protected virtual List<Func<CombatArgs, float>> DodgeRate { get; }

        public CombatSet(
            List<Func<CombatArgs, float>> hardRate,
            List<Func<CombatArgs, float>> hardDamageRatio,
            List<Func<CombatArgs, float>> criticalRate,
            List<Func<CombatArgs, float>> criticalDamageRatio,
            List<Func<CombatArgs, float>> mpDamage,
            List<Func<CombatArgs, float>> mpCounteract,
            List<Func<CombatArgs, float>> dodgeRate)
        {
            HardRate = hardRate;
            HardDamageRatio = hardDamageRatio;
            CriticalRate = criticalRate;
            CriticalDamageRatio = criticalDamageRatio;
            MpDamage = mpDamage;
            MpCounteract = mpCounteract;
            DodgeRate = dodgeRate;
        }

        public virtual float GetHardRate(CombatArgs arg) => HardRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public virtual float GetHardDamageRatio(CombatArgs arg) => HardDamageRatio?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public virtual float GetCriticalRate(CombatArgs arg) => CriticalRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public virtual float GetCriticalDamageRatio(CombatArgs arg) => CriticalDamageRatio?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public virtual float GetMpDamage(CombatArgs arg) => MpDamage?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public virtual float GetMpCounteract(CombatArgs arg) => MpCounteract?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public virtual float GetDodgeRate(CombatArgs arg) => DodgeRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
    }
}