using System;
using System.Collections.Generic;
using System.Linq;
using GameClient.Args;

namespace GameClient.Modules.BattleM
{
    /// <summary>
    /// 战斗技能(武学)的实例
    /// </summary>
    public record CombatSet : ICombatSet
    {
        internal static readonly CombatSet Empty = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, null);
        private List<Func<CombatArgs, float>> HardRate { get; }
        private List<Func<CombatArgs, float>> HardDamageRatio { get; }
        private List<Func<CombatArgs, float>> CriticalRate { get; }
        private List<Func<CombatArgs, float>> CriticalDamageRatio { get; }
        private List<Func<CombatArgs, float>> MpUses { get; }
        private List<Func<CombatArgs, float>> MpArmor { get;  }
        private List<Func<CombatArgs, float>> MpDamageCovertRateAddOn { get; }
        private List<Func<CombatArgs, float>> DamageMpArmorRate { get; }
        private List<Func<CombatArgs, float>> MpArmorConvertRateAddOn { get; }
        private List<Func<CombatArgs, float>> DodgeRate { get; }
        private List<Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> SelfBuffs { get; }
        private List<Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> TargetBuffs { get; }

        public CombatSet(
            List<Func<CombatArgs, float>> hardRate,
            List<Func<CombatArgs, float>> hardDamageRatio,
            List<Func<CombatArgs, float>> criticalRate,
            List<Func<CombatArgs, float>> criticalDamageRatio,
            List<Func<CombatArgs, float>> mpUses,
            List<Func<CombatArgs, float>> mpArmor,
            List<Func<CombatArgs, float>> mpDamageCovertRateAddOn, 
            List<Func<CombatArgs, float>> damageMpArmorRate,
            List<Func<CombatArgs, float>> mpArmorConvertRateAddOn,
            List<Func<CombatArgs, float>> dodgeRate, 
            List<Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> selfBuffs, 
            List<Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> targetBuffs
            )
        {
            HardRate = hardRate;
            HardDamageRatio = hardDamageRatio;
            CriticalRate = criticalRate;
            CriticalDamageRatio = criticalDamageRatio;
            MpUses = mpUses;
            DamageMpArmorRate = damageMpArmorRate;
            DodgeRate = dodgeRate;
            MpArmor = mpArmor;
            MpDamageCovertRateAddOn = mpDamageCovertRateAddOn;
            MpArmorConvertRateAddOn = mpArmorConvertRateAddOn;
            SelfBuffs = selfBuffs?.Where(b=>b!=null).ToList();
            TargetBuffs = targetBuffs?.Where(b=>b!=null).ToList();
        }

        public CombatSet(
            Func<CombatArgs, float> hardRate,
            Func<CombatArgs, float> hardDamageRatio,
            Func<CombatArgs, float> criticalRate,
            Func<CombatArgs, float> criticalDamageRatio,
            Func<CombatArgs, float> mpDamage,
            Func<CombatArgs, float> mpArmor,
            Func<CombatArgs, float> mpDamageCovertRate,
            Func<CombatArgs, float> mpArmorRate,
            Func<CombatArgs, float> mpArmorConvertRate,
            Func<CombatArgs, float> dodgeRate, 
            Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>> selfBuffs, 
            Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>> targetBuffs) 
            : this(
            new List<Func<CombatArgs, float>> { hardRate },
            new List<Func<CombatArgs, float>> { hardDamageRatio },
            new List<Func<CombatArgs, float>> { criticalRate },
            new List<Func<CombatArgs, float>> { criticalDamageRatio },
            new List<Func<CombatArgs, float>> { mpDamage },
            new List<Func<CombatArgs, float>> { mpArmor },
            new List<Func<CombatArgs, float>> { mpDamageCovertRate },
            new List<Func<CombatArgs, float>> { mpArmorRate },
            new List<Func<CombatArgs, float>> { mpArmorConvertRate },
            new List<Func<CombatArgs, float>> { dodgeRate }, 
            new List<Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>>{selfBuffs}, 
            new List<Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>> { targetBuffs }
            ) { }

        public CombatSet(float hardRate,
            float hardDamageRatioAddOn,
            float criticalRate,
            float criticalDamageRatioAddOn,
            float mpDamage,
            float mpArmor,
            float mpDamageCovertRate,
            float mpArmorRate,
            float mpArmorConvertRate,
            float dodgeRate,
            Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>> selfBuffs,
            Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>
                targetBuffs)
            : this(
                _ => hardRate,
                _ => hardDamageRatioAddOn,
                _ => criticalRate,
                _ => criticalDamageRatioAddOn,
                _ => mpDamage,
                _ => mpArmor,
                _ => mpDamageCovertRate,
                _ => mpArmorRate,
                _ => mpArmorConvertRate,
                _ => dodgeRate,
                selfBuffs,
                targetBuffs) { }

        public float GetHardRate(CombatArgs arg) => HardRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetHardDamageRatioAddOn(CombatArgs arg) => HardDamageRatio?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetCriticalRate(CombatArgs arg) => CriticalRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetCriticalDamageRatioAddOn(CombatArgs arg) => CriticalDamageRatio?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetMpUses(CombatArgs arg) => MpUses?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetMpDamageConvertRateAddOn(CombatArgs arg)=> MpDamageCovertRateAddOn?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetDamageMpArmorRate(CombatArgs arg) => DamageMpArmorRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetMpArmor(CombatArgs arg)=> MpArmor?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetMpArmorConvertRateAddOn(CombatArgs arg) => MpArmorConvertRateAddOn?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public float GetDodgeRate(CombatArgs arg) => DodgeRate?.Sum(f => f?.Invoke(arg) ?? 0) ?? 0;
        public IEnumerable<CombatBuff> GetSelfBuffs(DiziCombatUnit caster, int round,
            BuffManager<DiziCombatUnit> buffManager) =>
            SelfBuffs?.Where(b => b != null).SelectMany(b => b.Invoke(caster, round, buffManager))
                .Where(b => b != null) ?? Enumerable.Empty<CombatBuff>();
        public IEnumerable<CombatBuff> GetTargetBuffs(DiziCombatUnit target, DiziCombatUnit caster, CombatArgs args,
            int round,
            BuffManager<DiziCombatUnit> buffManager) =>
            TargetBuffs?.Where(b => b != null).SelectMany(s => s.Invoke(target, caster, args, round, buffManager))
                .Where(b => b != null) ?? Enumerable.Empty<CombatBuff>();
    }
}