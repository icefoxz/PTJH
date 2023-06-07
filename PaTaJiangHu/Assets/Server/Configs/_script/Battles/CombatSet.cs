using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Configs.Battles
{
    /// <summary>
    /// 战斗技能(武学)的实例
    /// </summary>
    public record CombatSet : ICombatSet
    {
        internal static readonly CombatSet Empty = new(0, 0, 0, 0, 0, 0, 0, null, null);
        private List<Func<CombatArgs, float>> HardRate { get; }
        private List<Func<CombatArgs, float>> HardDamageRatio { get; }
        private List<Func<CombatArgs, float>> CriticalRate { get; }
        private List<Func<CombatArgs, float>> CriticalDamageRatio { get; }
        private List<Func<CombatArgs, float>> MpDamage { get; }
        private List<Func<CombatArgs, float>> MpCounteract { get; }
        private List<Func<CombatArgs, float>> DodgeRate { get; }
        private List<Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> SelfBuffs { get; }
        private List<Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> TargetBuffs { get; }

        public CombatSet(
            List<Func<CombatArgs, float>> hardRate,
            List<Func<CombatArgs, float>> hardDamageRatio,
            List<Func<CombatArgs, float>> criticalRate,
            List<Func<CombatArgs, float>> criticalDamageRatio,
            List<Func<CombatArgs, float>> mpDamage,
            List<Func<CombatArgs, float>> mpCounteract,
            List<Func<CombatArgs, float>> dodgeRate, 
            List<Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> selfBuffs, 
            List<Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> targetBuffs)
        {
            HardRate = hardRate;
            HardDamageRatio = hardDamageRatio;
            CriticalRate = criticalRate;
            CriticalDamageRatio = criticalDamageRatio;
            MpDamage = mpDamage;
            MpCounteract = mpCounteract;
            DodgeRate = dodgeRate;
            SelfBuffs = selfBuffs?.Where(b=>b!=null).ToList();
            TargetBuffs = targetBuffs?.Where(b=>b!=null).ToList();
        }

        public CombatSet(
            Func<CombatArgs, float> hardRate,
            Func<CombatArgs, float> hardDamageRatio,
            Func<CombatArgs, float> criticalRate,
            Func<CombatArgs, float> criticalDamageRatio,
            Func<CombatArgs, float> mpDamage,
            Func<CombatArgs, float> mpCounteract,
            Func<CombatArgs, float> dodgeRate, 
            Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>> selfBuffs, 
            Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>> targetBuffs) : this(
            new List<Func<CombatArgs, float>> { hardRate },
            new List<Func<CombatArgs, float>> { hardDamageRatio },
            new List<Func<CombatArgs, float>> { criticalRate },
            new List<Func<CombatArgs, float>> { criticalDamageRatio },
            new List<Func<CombatArgs, float>> { mpDamage },
            new List<Func<CombatArgs, float>> { mpCounteract },
            new List<Func<CombatArgs, float>> { dodgeRate }, 
            new List<Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>>{selfBuffs}, 
            new List<Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> { targetBuffs }) { }

        public CombatSet(float hardRate,
            float hardDamageRatio,
            float criticalRate,
            float criticalDamageRatio,
            float mpDamage,
            float mpCounteract,
            float dodgeRate, 
            Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>> selfBuffs, 
            Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>> targetBuffs) 
            : this(_=>hardRate,
            _=>hardDamageRatio,
            _=>criticalRate,
            _=>criticalDamageRatio,
            _=>mpDamage,
            _=>mpCounteract,
            _=>dodgeRate, 
            selfBuffs, 
            targetBuffs) { }

        public float GetHardRate(CombatArgs arg) => HardRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetHardDamageRatio(CombatArgs arg) => HardDamageRatio?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetCriticalRate(CombatArgs arg) => CriticalRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetCriticalDamageRatio(CombatArgs arg) => CriticalDamageRatio?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetMpDamage(CombatArgs arg) => MpDamage?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetMpCounteract(CombatArgs arg) => MpCounteract?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetDodgeRate(CombatArgs arg) => DodgeRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public IEnumerable<CombatBuff> GetSelfBuffs(DiziCombatUnit caster, int round,
            BuffManager<DiziCombatUnit> buffManager) =>
            SelfBuffs?.Where(b=>b!=null).SelectMany(b => b.Invoke(caster, round, buffManager)).Where(b=>b!=null) ?? Enumerable.Empty<CombatBuff>();

        public IEnumerable<CombatBuff> GetTargetBuffs(DiziCombatUnit target, DiziCombatUnit caster, CombatArgs args,
            int round,
            BuffManager<DiziCombatUnit> buffManager) =>
            TargetBuffs?.Where(b => b != null).SelectMany(s => s.Invoke(target, caster, args, round, buffManager))
                .Where(b => b != null) ?? Enumerable.Empty<CombatBuff>();
    }
}