using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Server.Configs.Battles;
using Server.Configs.Items;
using UnityEngine;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "levelStrSo", menuName = "战斗/武学/等级策略")]
    public class SkillLevelStrategySo : AutoAtNamingObject
    {
        [SerializeField]private LevelingField[] 等级;
        private LevelingField[] LevelingFields => 等级;

        public ICombatSet GetCombatSet(int level)
        {
            var index = level - 1;
            if (index < 0 || index >= LevelingFields.Length)
                throw new ArgumentOutOfRangeException($"{nameof(GetCombatSet)}.找不到等级配置:{name}.等级 = {index + 1}, 索引 = {index}");
            return LevelingFields[index].GetCombat();
        }

        public ISkillAttribute[] GetAttributes(int level)
        {
            var index = level - 1;
            if (index < 0 || index >= LevelingFields.Length)
                throw new ArgumentOutOfRangeException($"{nameof(GetAttributes)}.找不到等级配置:{name}.等级 = {level}, 索引 = {index}");
            return LevelingFields[index].GetAttributes();
        }

        public ISkillProp[] GetProps(int level)
        {
            var index = level - 1;
            if (index < 0 || index >= LevelingFields.Length)
                throw new ArgumentOutOfRangeException($"{nameof(GetProps)}.找不到等级配置:{name}.等级 = {level}, 索引 = {index}");
            return LevelingFields[index].GetProps();
        }

        public int MaxLevel() => LevelingFields.Length;
        [Serializable] private class LevelingField
        {
            private bool SetName()
            {
                var propText = string.Empty;
                if (Fields?.Length > 0)
                    propText = string.Join(',', Fields.Select(f => f.Name));
                var diffText = string.Empty;
                diffText = DifferentialStrategies.Any(s => s == null) ? "文件异常!请检查空文件!" : string.Join(',', DifferentialStrategies.Select(f => f.name));
                _name = $"等级【{Level}】{propText}|{diffText}";
                return true;
            }

            [ConditionalField(true, nameof(SetName))][SerializeField][ReadOnly] private string _name;
            [SerializeField] [Min(1)] private int 等级 = 1;
            [SerializeField] private CombatAdvancePropField[] 属性;
            [SerializeField] private CombatCompareStrategySo[] 差值配置;
            [SerializeField] private EffectBuffSoBase[] _buffs;

            private EffectBuffSoBase[] Buffs => _buffs;
            private CombatCompareStrategySo[] DifferentialStrategies => 差值配置;
            private CombatAdvancePropField[] Fields => 属性;
            private int Level => 等级;

            public ICombatSet GetCombat()
            {
                var hardRateList = new List<Func<CombatArgs, float>>();
                var hardDamageRatioList = new List<Func<CombatArgs, float>>();
                var criticalRateList = new List<Func<CombatArgs, float>>();
                var criticalDamageRatioList = new List<Func<CombatArgs, float>>();
                var mpDamageList = new List<Func<CombatArgs, float>>();
                var mpCounteractList = new List<Func<CombatArgs, float>>();
                var dodgeRateList = new List<Func<CombatArgs, float>>();
                var selfBuffList = new List<Func<DiziCombatUnit, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>>();
                var targetBuffList = new List<Func<DiziCombatUnit, DiziCombatUnit, CombatArgs, int, BuffManager<DiziCombatUnit>, IEnumerable<CombatBuff>>>();

                //值叠加
                foreach (var field in Fields)
                {
                    hardRateList.Add(_ => field.HardRate);
                    hardDamageRatioList.Add(_ => field.HardDamageRateAddOn);
                    criticalRateList.Add(_ => field.CriticalRate);
                    criticalDamageRatioList.Add(_ => field.CriticalDamageRateAddOn);
                    dodgeRateList.Add(_ => field.DodgeRate);
                    mpDamageList.Add(_ => field.MpDamage);
                    mpCounteractList.Add(_ => field.MpCounteract);
                }

                foreach (var buff in Buffs)
                {
                    selfBuffList.Add(buff.InstanceSelfBuffs);
                    targetBuffList.Add(buff.InstanceTargetBuffs);
                }

                //公式叠加
                foreach (var strategy in DifferentialStrategies)
                {
                    switch (strategy.Set)
                    {
                        case CombatCompareStrategySo.Settings.HardRate:
                            hardRateList.Add(strategy.GetHardRate);
                            break;
                        case CombatCompareStrategySo.Settings.HardDamageRate:
                            hardDamageRatioList.Add(strategy.GetHardDamageRatio);
                            break;
                        case CombatCompareStrategySo.Settings.CriticalRate:
                            criticalRateList.Add(strategy.GetCriticalRate);
                            break;
                        case CombatCompareStrategySo.Settings.DogeRate:
                            dodgeRateList.Add(strategy.GetDodgeRate);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return new CombatSet(
                    hardRate: hardRateList,
                    hardDamageRatio: hardDamageRatioList,
                    criticalRate: criticalRateList,
                    criticalDamageRatio: criticalDamageRatioList,
                    mpDamage: mpDamageList,
                    mpCounteract: mpCounteractList,
                    dodgeRate: dodgeRateList,
                    selfBuffs: selfBuffList,
                    targetBuffs: targetBuffList);
            }

            public ISkillAttribute[] GetAttributes() => DifferentialStrategies.Select(d => d.GetCombatAttribute()).ToArray();

            public ISkillProp[] GetProps() => Fields.SelectMany(f => f.GetProps()).ToArray();
        }
    }

    /// <summary>
    /// 战斗高级属性配置
    /// </summary>
    [Serializable] internal class CombatAdvancePropField
    {
        public enum Settings
        {
            [InspectorName("内力")] Force,
            [InspectorName("闪避")] Dodge,
            [InspectorName("重击")] Hard,
            [InspectorName("会心")] Critical,
        }

        private bool SetLevelName()
        {
            var texts = GetSettingTexts();
            _name = $"[{GetNameText(_set)}]{string.Join(',', texts)}";
            return true;

        }

        private string[] GetSettingTexts()
        {
            return _set switch
            {
                Settings.Force => SetForceText(),
                Settings.Dodge => SetDodgeText(),
                Settings.Hard => SetHardText(),
                Settings.Critical => SetCritical(),
                //Settings.Special => SetSpecial(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private string GetNameText(Settings set) => set switch
        {
            Settings.Force => "内力",
            Settings.Dodge => "闪避",
            Settings.Hard => "重击",
            Settings.Critical => "会心",
            //Settings.Special => "特殊",
            _ => throw new ArgumentOutOfRangeException(nameof(set), set, null)
        };

        private string[] SetSpecial() => new[] { "特殊招式未支持" };

        [ConditionalField(true, nameof(SetLevelName))]
        [SerializeField]
        [ReadOnly]
        private string _name;

        [SerializeField] private Settings _set;

        #region Hard

        [ConditionalField(nameof(_set), false, Settings.Hard)]
        [SerializeField]
        private float 重击率;

        [ConditionalField(nameof(_set), false, Settings.Hard)]
        [SerializeField]
        private float 重倍加成;

        public float HardRate => 重击率;
        public float HardDamageRateAddOn => 重倍加成 * 0.01f;
        private string[] SetHardText() => new[]{$"触发:{ResolveSymbol(HardRate)}%" ,
                                                     $"倍率:{ResolveSymbol(HardDamageRateAddOn)}"};

        #endregion

        #region dodge

        [ConditionalField(nameof(_set), false, Settings.Dodge)]
        [SerializeField]
        private float 闪避率;

        public float DodgeRate => 闪避率;
        private string[] SetDodgeText() => new[] { $"触发:{ResolveSymbol(DodgeRate)}%" };

        #endregion

        #region force

        [ConditionalField(nameof(_set), false, Settings.Force)]
        [SerializeField]
        private float 内力消耗;

        public float MpDamage => 内力消耗;

        [ConditionalField(nameof(_set), false, Settings.Force)]
        //[SerializeField]
        private float 内力抵消;

        public float MpCounteract => 内力抵消;
        private string[] SetForceText() => new[]
        {
                $"消耗:{ResolveSymbol(MpDamage)}"
                //, $"抵消:{MpCounteract}"
            };

        #endregion

        #region critical

        [ConditionalField(nameof(_set), false, Settings.Critical)]
        [SerializeField]
        private float 会心率;
        [ConditionalField(nameof(_set), false, Settings.Critical)]
        [SerializeField]
        private float 会心倍率;

        public float CriticalRate => 会心率;
        public float CriticalDamageRateAddOn => 会心倍率 * 0.01f;
        public string Name => _name;

        private string[] SetCritical() => new[]{ $"触发:{ResolveSymbol(CriticalRate)}%" ,
                                                      $"倍率:{ResolveSymbol(CriticalDamageRateAddOn)}"};
        #endregion
        private static string ResolveSymbol(float value)
        {
            if (value == 0) return value.ToString();
            return value > 0 ? $"+{value}" : $"-{value}";
        }

        public ISkillProp[] GetProps()
        {
            var label = GetNameText(_set);
            var settings = GetSettingTexts();
            return settings.Select(s => new SkillProp(label, s)).Cast<ISkillProp>().ToArray();
        }

        private record SkillProp(string Name, string Value) : ISkillProp
        {
            public string Name { get; } = Name;
            public string Value { get; } = Value;
        }

        public ICombatSet GetCombatSet() => new CombatSet(
            HardRate, 
            HardDamageRateAddOn, 
            CriticalRate,
            CriticalDamageRateAddOn, 
            MpDamage, 
            MpCounteract, 
            DodgeRate,
            null,
            null
            );
    }
}