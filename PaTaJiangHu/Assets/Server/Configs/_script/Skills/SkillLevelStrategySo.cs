using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Server.Configs.Battles;
using Server.Configs.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "levelStrSo", menuName = "战斗/武学/等级策略")]
    public class SkillLevelStrategySo : AutoAtNamingObject
    {
        [SerializeField]private LevelingField[] 等级;
        private LevelingField[] LevelingFields => 等级;

        public ICombatSet GetCombatSet(int index)
        {
            if (index < 0 || index >= LevelingFields.Length)
                throw new ArgumentOutOfRangeException($"找不到等级配置:{name}.等级 = {index + 1}, 索引 = {index}");
            return LevelingFields[index].GetCombat();
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

            private string SetSpecial() => "特殊招式未支持";

            [ConditionalField(true, nameof(SetName))][SerializeField][ReadOnly] private string _name;
            [SerializeField] [Min(1)] private int 等级 = 1;
            [SerializeField] private CombatField[] 属性;
            [SerializeField] private CombatDifferentialStrategySo[] 差值配置;

            private CombatDifferentialStrategySo[] DifferentialStrategies => 差值配置;
            private CombatField[] Fields => 属性;
            private int Level => 等级;

            public ICombatSet GetCombat()
            {
                var hardRate = 0f;
                var hardDamageRatio = 1f;
                var criticalRate = 0f;
                var criticalMultiplier = 0f;
                var mpDamage = 0f;
                var mpCounteract = 0f;
                var dodgeRate = 0f;
                foreach (var field in Fields)
                {
                    hardRate += field.HardRate;
                    hardDamageRatio += field.HardDamageRateAddOn * 0.01f;
                    criticalRate += field.CriticalRate;
                    criticalMultiplier += field.CriticalMultiplier;
                    mpDamage += field.MpDamage;
                    mpCounteract += field.MpCounteract;
                    dodgeRate += field.DodgeRate;
                }

                var hardRateList = new List<Func<CombatArgs, float>>();
                var hardDamageRatioList = new List<Func<CombatArgs, float>>();
                var criticalRateList = new List<Func<CombatArgs, float>>();
                var criticalMultiplierList = new List<Func<CombatArgs, float>>();
                var mpDamageList = new List<Func<CombatArgs, float>>();
                var mpCounteractList = new List<Func<CombatArgs, float>>();
                var dodgeRateList = new List<Func<CombatArgs, float>>();

                //值叠加
                foreach (var field in Fields)
                {
                    hardRateList.Add(_ => field.HardRate);
                    hardDamageRatioList.Add(_ => field.HardDamageRateAddOn);
                    criticalRateList.Add(_ => field.CriticalRate);
                    criticalMultiplierList.Add(_ => field.CriticalMultiplier);
                    dodgeRateList.Add(_ => field.DodgeRate);
                    mpDamageList.Add(_ => field.MpDamage);
                    mpCounteractList.Add(_ => field.MpCounteract);
                }

                //公式叠加
                foreach (var strategy in DifferentialStrategies)
                {
                    switch (strategy.Set)
                    {
                        case CombatDifferentialStrategySo.Settings.HardRate:
                            hardRateList.Add(strategy.GetHardRate);
                            break;
                        case CombatDifferentialStrategySo.Settings.HardDamageRate:
                            hardDamageRatioList.Add(strategy.GetHardDamageRatio);
                            break;
                        case CombatDifferentialStrategySo.Settings.CriticalRate:
                            criticalRateList.Add(strategy.GetCriticalRate);
                            break;
                        case CombatDifferentialStrategySo.Settings.DogeRate:
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
                    criticalMultiplier: criticalMultiplierList,
                    mpDamage: mpDamageList,
                    mpCounteract: mpCounteractList,
                    dodgeRate: dodgeRateList);
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
            public string Name => _name;
            [SerializeField] private Settings _set;

            #region Hard

            [ConditionalField(nameof(_set), false, Settings.Hard)] [SerializeField]
            private float 重击率;

            [ConditionalField(nameof(_set), false, Settings.Hard)] [SerializeField]
            private float 重倍加成;

            public float HardRate => 重击率;
            public float HardDamageRateAddOn => 重倍加成 * 0.01f;
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

            [ConditionalField(nameof(_set), false, Settings.Force)] 
            //[SerializeField]
            private float 内力抵消;

            public float MpCounteract => 内力抵消;
            private string SetForceText() => $"消耗:{MpDamage}" 
                                             //+ $", 抵消:{MpCounteract}"
                                             ;

            #endregion

            #region critical

            [ConditionalField(nameof(_set), false, Settings.Critical)] [SerializeField]
            private float 会心率;
            [ConditionalField(nameof(_set), false, Settings.Critical)] [SerializeField]
            private float 会心倍率;

            public float CriticalRate => 会心率;
            public float CriticalMultiplier => 会心倍率;

            private string SetCritical() => $"触发:{CriticalRate}%,倍率x{CriticalMultiplier}";

            #endregion
        }
    }
}