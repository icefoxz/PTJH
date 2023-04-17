using System;
using System.Collections.Generic;
using MyBox;
using Server.Configs.Battles;
using Server.Configs.Items;
using UnityEngine;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "levelStrSo", menuName = "战斗/武学/等级策略")]
    public class SkillLevelStrategySo : AutoAtNamingObject
    {
        #region ReferenceSo
        private bool ReferenceSo()
        {
            _so = this;
            return true;
        }

        [ConditionalField(true, nameof(ReferenceSo))] [ReadOnly] [SerializeField] private ScriptableObject _so;
        #endregion

        [SerializeField]private LevelingField[] 等级;
        private LevelingField[] LevelingFields => 等级;

        public ICombatSet GetCombatSet(int index)
        {
            if (index < 0 || index >= LevelingFields.Length)
                throw new ArgumentOutOfRangeException($"找不到等级配置:{name}.等级 = {index + 1}, 索引 = {index}");
            return LevelingFields[index].GetCombat();
        }

        public int MaxLevel() => LevelingFields.Length;
        [Serializable]
        private class LevelingField
        {
            [SerializeField] private CombatField[] 属性;
            [SerializeField] private CombatDifferentialStrategy[] 差值配置;

            private CombatDifferentialStrategy[] DifferentialStrategies => 差值配置;
            private CombatField[] Fields => 属性;
            public ICombatSet GetCombat()
            {
                var hardRate = 0f;
                var hardDamageRatio = 1f;
                var criticalRate = 0f;
                var mpDamage = 0f;
                var mpCounteract = 0f;
                var dodgeRate = 0f;
                foreach (var field in Fields)
                {
                    hardRate += field.HardRate;
                    hardDamageRatio += field.HardDamageRateAddOn * 0.01f;
                    criticalRate += field.CriticalRate;
                    mpDamage += field.MpDamage;
                    mpCounteract += field.MpCounteract;
                    dodgeRate += field.DodgeRate;
                }

                var hardRateList = new List<Func<CombatArgs, float>>();
                var hardDamageRatioList = new List<Func<CombatArgs, float>>();
                var criticalRateList = new List<Func<CombatArgs, float>>();
                var mpDamageList = new List<Func<CombatArgs, float>>();
                var mpCounteractList = new List<Func<CombatArgs, float>>();
                var dodgeRateList = new List<Func<CombatArgs, float>>();

                //值叠加
                foreach (var field in Fields)
                {
                    hardRateList.Add(_ => field.HardRate);
                    hardDamageRatioList.Add(_ => field.HardDamageRateAddOn);
                    criticalRateList.Add(_ => field.CriticalRate);
                    dodgeRateList.Add(_ => field.DodgeRate);
                    mpDamageList.Add(_ => field.MpDamage);
                    mpCounteractList.Add(_ => field.MpCounteract);
                }

                //公式叠加
                foreach (var strategy in DifferentialStrategies)
                {
                    switch (strategy.Set)
                    {
                        case CombatDifferentialStrategy.Settings.HardRate:
                            hardRateList.Add(strategy.GetHardRate);
                            break;
                        case CombatDifferentialStrategy.Settings.HardDamageRate:
                            hardDamageRatioList.Add(strategy.GetHardDamageRatio);
                            break;
                        case CombatDifferentialStrategy.Settings.CriticalRate:
                            criticalRateList.Add(strategy.GetCriticalRate);
                            break;
                        case CombatDifferentialStrategy.Settings.DogeRate:
                            dodgeRateList.Add(strategy.GetDodgeRate);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return new CombatSet(
                    hardRateList,
                    hardDamageRatioList,
                    criticalRateList,
                    mpDamageList,
                    mpCounteractList,
                    dodgeRateList);
            }

            [Serializable] private class CombatDifferentialStrategy
            {
                public enum Calculate
                {
                    [InspectorName("除")]Divide,
                    [InspectorName("乘")]Multiply,
                }
                public enum Compares
                {
                    [InspectorName("力")] Strength,
                    [InspectorName("敏")] Agility,
                    [InspectorName("血")] Hp,
                    [InspectorName("血上限")] HpMax,
                    [InspectorName("内")] Mp,
                    [InspectorName("内上限")] MpMax,
                }
                public enum Settings
                {
                    [InspectorName("重击触发")]HardRate,
                    [InspectorName("重击倍率")]HardDamageRate,
                    [InspectorName("会心触发")]CriticalRate,
                    [InspectorName("闪避触发")]DogeRate
                }
                private bool SetLevelName()
                {
                    var text = _set switch
                    {
                        Settings.HardRate => "重击触发",
                        Settings.HardDamageRate => "重击倍率",
                        Settings.CriticalRate => "会心触发",
                        Settings.DogeRate => "闪避触发",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    var compare = Compare switch
                    {
                        Compares.Strength => "力",
                        Compares.Agility => "敏",
                        Compares.Hp => "血",
                        Compares.HpMax => "血上限",
                        Compares.Mp => "内",
                        Compares.MpMax => "内上限",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    var cal = Cal switch
                    {
                        Calculate.Multiply => "乘",
                        Calculate.Divide => "除",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    var offsetText = Offset == 0 ? string.Empty
                        : Offset > 0 ? $"+{Offset}"
                        : $"{Offset}";

                    _name = $"[{text}]{compare}差{offsetText}: ({cal})系数({Factor})";
                    return true;
                }

                private string SetSpecial() => "特殊招式未支持";

                [ConditionalField(true, nameof(SetLevelName))] [SerializeField] [ReadOnly] private string _name;

                [SerializeField] private Settings _set;
                [SerializeField] private Compares 差值;
                [SerializeField] private float 校正;
                [SerializeField] private Calculate 计算;
                [SerializeField] private float 系数;

                public Settings Set => _set;
                public Compares Compare => 差值;
                public Calculate Cal => 计算;
                public float Factor => 系数;
                public float Offset => 校正;

                #region Calculate
                private float Calculation(CombatArgs arg)
                {
                    return Cal switch
                    {
                        Calculate.Divide => DivideFactor(arg),
                        Calculate.Multiply => MultiplyFactor(arg),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    float DivideFactor(CombatArgs a)
                    {
                        var caster = GetCombatValue(a.Caster);
                        var target = GetCombatValue(a.Target);
                        return (caster - target + Offset) / Factor;
                    }

                    float MultiplyFactor(CombatArgs a)
                    {
                        var caster = GetCombatValue(a.Caster);
                        var target = GetCombatValue(a.Target);
                        return (caster - target + Offset) * Factor;
                    }
                }

                private float GetCombatValue(DiziCombatUnit dizi) =>
                    Compare switch
                    {
                        Compares.Strength => dizi.Strength,
                        Compares.Agility => dizi.Agility,
                        Compares.Hp => dizi.Hp,
                        Compares.HpMax => dizi.MaxHp,
                        Compares.Mp => dizi.Mp,
                        Compares.MpMax => dizi.MaxMp,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                #endregion

                public float GetHardRate(CombatArgs arg) => Calculation(arg);
                public float GetHardDamageRatio(CombatArgs arg) => Calculation(arg);
                public float GetCriticalRate(CombatArgs arg) => Calculation(arg);
                public float GetDodgeRate(CombatArgs arg) => Calculation(arg);
            }
        }

        [Serializable]
        internal class CombatField
        {
            public enum Settings
            {
                [InspectorName("内力")] Force,
                [InspectorName("闪避")] Dodge,
                [InspectorName("重击")] Hard,
                [InspectorName("会心")] Critical,
                [InspectorName("特殊")] Special,
            }

            private bool SetLevelName()
            {
                var text = _set switch
                {
                    Settings.Force => SetForceText(),
                    Settings.Dodge => SetDodgeText(),
                    Settings.Hard => SetHardText(),
                    Settings.Critical => SetCritical(),
                    Settings.Special => SetSpecial(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                _name = $"[{GetSettings(_set)}]{text}";
                return true;

                string GetSettings(Settings set) => set switch
                {
                    Settings.Force => "内力",
                    Settings.Dodge => "闪避",
                    Settings.Hard => "重击",
                    Settings.Critical => "会心",
                    Settings.Special => "特殊",
                    _ => throw new ArgumentOutOfRangeException(nameof(set), set, null)
                };
            }

            private string SetSpecial() => "特殊招式未支持";

            [ConditionalField(true, nameof(SetLevelName))] [SerializeField] [ReadOnly]
            private string _name;

            [SerializeField] private Settings _set;

            #region Hard

            [ConditionalField(nameof(_set), false, Settings.Hard)] [SerializeField]
            private float 重击率;

            [ConditionalField(nameof(_set), false, Settings.Hard)] [SerializeField]
            private float 重击倍率加成 = 25f;

            public float HardRate => 重击率;
            public float HardDamageRateAddOn => 重击倍率加成 * 0.01f;
            private string SetHardText() => $"触发:{HardRate}%,倍率:{1 + HardDamageRateAddOn}";

            #endregion

            #region dodge

            [ConditionalField(nameof(_set), false, Settings.Dodge)] [SerializeField]
            private float 闪避率;

            public float DodgeRate => 闪避率;
            private string SetDodgeText() => $"触发:{DodgeRate}%";

            #endregion

            #region force

            [ConditionalField(nameof(_set), false, Settings.Force)] [SerializeField]
            private float 内力消耗;

            public float MpDamage => 内力消耗;

            [ConditionalField(nameof(_set), false, Settings.Force)] [SerializeField]
            private float 内力抵消;

            public float MpCounteract => 内力抵消;
            private string SetForceText() => $"消耗:{MpDamage}, 抵消:{MpCounteract}";

            #endregion

            #region critical

            [ConditionalField(nameof(_set), false, Settings.Critical)] [SerializeField]
            private float 会心率;

            public float CriticalRate => 会心率;
            private string SetCritical() => $"触发:{CriticalRate}%";

            #endregion
        }
    }
}