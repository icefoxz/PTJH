using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Server.Configs.Battles;
using Server.Configs.Characters;
using Server.Configs.Items;
using UnityEngine;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "buff", menuName = "战斗/buff")]
    [Serializable]
    public class EffectBuffSo : EffectBuffSoBase
    {
        private enum BuffTargets
        {
            [InspectorName("自己")] Self,
            [InspectorName("对方")] Target
        }
        private enum Functions
        {
            [InspectorName("恢复血量")] RecoverHp,
            [InspectorName("恢复内力")] RecoverMp,
            [InspectorName("扣除血量")] SubtractHp,
            [InspectorName("扣除内力")] SubtractMp,
        }

        private enum RoundTriggers
        {
            [InspectorName("无")] None,
            [InspectorName("回合开始")] RoundStart,
            [InspectorName("回合结束")] RoundEnd,
        }

        [SerializeField] private BuffTargets _buff目标;
        [SerializeField] private bool 允许叠加;
        [SerializeField] private PropBeforeRoundField[] 回合前属性;
        [SerializeField] private RoundTriggers 回合触发;
        [ConditionalField(nameof(回合触发),true,RoundTriggers.None)][SerializeField] private RoundTriggerField 回合触发配置;
        private RoundTriggerField RoundField => 回合触发配置;
        private RoundTriggers RoundTrigger => 回合触发;
        private PropBeforeRoundField[] PropFields => 回合前属性;
        public override bool IsAllowStacking => 允许叠加;
        private BuffTargets BuffTarget => _buff目标;

        public override float GetStackingValue(string @event, DiziCombatUnit target) =>
            PropFields.Where(p => IsPropMatch(p.Prop, @event)).Sum(p => p.GetValue(target));

        public override void OnTriggeredFunction(DiziCombatUnit target, DiziCombatUnit caster, CombatArgs args,
            BuffManager<DiziCombatUnit> buffManager, List<DiziCombatUnit> units)
        {
            var recoverHpValue = 0f;
            var recoverMpValue = 0f;
            var subtractHpValue = 0f;
            var subtractMpValue = 0f;
            foreach (var field in RoundField.ConditionFunctionFields)
            {
                switch (field.Function)
                {
                    case Functions.RecoverHp:
                        recoverHpValue += field.GetEffectValue(target, caster,field.Function ,args);
                        break;
                    case Functions.RecoverMp:
                        recoverMpValue += field.GetEffectValue(target, caster,field.Function, args);
                        break;
                    case Functions.SubtractHp:
                        subtractHpValue += field.GetEffectValue(target, caster, field.Function, args);
                        break;
                    case Functions.SubtractMp:
                        subtractMpValue += field.GetEffectValue(target, caster, field.Function, args);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            target.AddHp((int)recoverHpValue);
            target.AddMp((int)recoverMpValue);
            target.AddHp(-(int)subtractHpValue);
            target.AddMp(-(int)subtractMpValue);
        }

        public override IEnumerable<CombatBuff> InstanceSelfBuffs(DiziCombatUnit caster, int round,
            BuffManager<DiziCombatUnit> buffMgr)
        {
            var typeId = Id.ToString();
            return BuffTarget == BuffTargets.Self && // 目标为自己
                   (IsAllowStacking || caster.Buffs.All(b => b.TypeId != typeId)) // 不允许叠加时,如果已经有同名buff,则不添加
                ? new[] { InstanceBuff(caster, buffMgr, null) }
                : Array.Empty<CombatBuff>();
        }

        public override IEnumerable<CombatBuff> InstanceTargetBuffs(DiziCombatUnit target, DiziCombatUnit caster, CombatArgs args, int round,
            BuffManager<DiziCombatUnit> buffMgr)
        {
            var typeId = Id.ToString();
            return BuffTarget == BuffTargets.Target && // 目标为对方
                   (IsAllowStacking || target.Buffs.All(b => b.TypeId != typeId)) // 不允许叠加时,如果已经有同名buff,则不添加
                ? new[] { InstanceBuff(caster, buffMgr, args) }
                : Array.Empty<CombatBuff>();
        }

        public CombatBuff InstanceBuff(DiziCombatUnit caster, BuffManager<DiziCombatUnit> buffManager, CombatArgs args)
        {
            return RoundTrigger switch
            {
                RoundTriggers.None => new EffectBuff(Id.ToString(), caster, buffManager, args, this),
                RoundTriggers.RoundStart => new BuffRoundEffectByMode(Id.ToString(), caster, buffManager,
                    args, this,
                    Combat.RoundTriggers.RoundStart),
                RoundTriggers.RoundEnd => new BuffRoundEffectByMode(Id.ToString(), caster, buffManager,
                    args, this,
                    Combat.RoundTriggers.RoundEnd),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static bool IsPropMatch(DiziProps p, string text)
        {
            return text != null && text == p switch
            {
                DiziProps.Strength => Combat.Str,
                DiziProps.Agility => Combat.Agi,
                DiziProps.Hp => Combat.Hp,
                DiziProps.Mp => Combat.Mp,
                _ => throw new ArgumentOutOfRangeException(nameof(p), p, null)
            };
        }

        [Serializable]
        private class RoundTriggerField
        {
            [SerializeField] private ConditionFunctionField[] 回合触发配置;
            public ConditionFunctionField[] ConditionFunctionFields => 回合触发配置;
        }

        [Serializable]
        private class PropBeforeRoundField
        {
            [SerializeField] private DiziProps 属性;
            [SerializeField] private float 值;
            [SerializeField] private bool 是百分比;
            public DiziProps Prop => 属性;
            public bool IsPercentage => 是百分比;

            public float Value => 值;

            public float GetValue(DiziCombatUnit target)
            {
                return !IsPercentage ? Value : 0.01f * Prop switch
                {
                    DiziProps.Strength => target.Strength.Value * Value,
                    DiziProps.Agility => target.Agility.Value * Value,
                    DiziProps.Hp => target.Hp.Max * Value,
                    DiziProps.Mp => target.Mp.Max * Value,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        // 状态功能配置
        [Serializable]
        private class ConditionFunctionField
        {
            [SerializeField] private Functions 功能;

            [Header("负数为百分比")]
            [ConditionalField(nameof(差值计算),true)] [SerializeField] private float 值 = 1;

            [SerializeField] private bool 差值计算;

            [ConditionalField(nameof(差值计算))] [SerializeField]
            private DiziProps 差值属性;

            [ConditionalField(nameof(差值计算))] [SerializeField]
            private Combat.Calculate 计算;

            [ConditionalField(nameof(差值计算))] [SerializeField] private float 系数;

            public Functions Function => 功能;
            private float Value => 值;
            private float Factor => 系数;

            private bool IsCompareCalculation => 差值计算;
            private DiziProps CompareProp => 差值属性;
            private Combat.Calculate Calculate => 计算;

            public float GetEffectValue(DiziCombatUnit target, DiziCombatUnit caster, Functions fieldFunction,
                CombatArgs args)
            {
                if (IsCompareCalculation)
                {
                    var (self, other) = args.GetUnits(target);
                    var value = CompareProp switch
                    {
                        DiziProps.Strength => self.Strength.Value - other.Strength.Value,
                        DiziProps.Agility => self.Agility.Value - other.Agility.Value,
                        DiziProps.Hp => self.Hp.Value - other.Hp.Value,
                        DiziProps.Mp => self.Mp.Value - other.Mp.Value,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    return Calculate switch
                    {
                        Combat.Calculate.Multiply => value * Factor,
                        Combat.Calculate.Divide => Factor == 0 ? 0 : value / Factor,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                return ValueConversion(fieldFunction, target, Value);
            }

        }

        private static float ValueConversion(Functions function, DiziCombatUnit target, float value)
        {
            return value switch
            {
                > 0 => value,
                < 0 => (value / -100 * function switch
                {
                    Functions.RecoverHp => target.Hp.Max,
                    Functions.RecoverMp => target.Mp.Max,
                    Functions.SubtractHp => target.Hp.Max,
                    Functions.SubtractMp => target.Mp.Max,
                    _ => throw new ArgumentOutOfRangeException()
                }),
                0 => 0,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    /// <summary>
    /// 触发类buff配置
    /// </summary>
    public abstract class EffectBuffSoBase : AutoDashNamingObject, IEffectBuffConfig
    {
        [SerializeField] private RoundEffectBuff.EffectModes 效果模式;
        [ConditionalField(nameof(效果模式),true, RoundEffectBuff.EffectModes.EveryRound)][SerializeField] private int 回合;

        [Header("持久值=回合数,自动递减至0后会去buff,-1不回合递减")] [SerializeField] private int 持久值 = 1;
        public RoundEffectBuff.EffectModes Mode => 效果模式;
        public int RoundCount => 回合;
        public int Lasting => 持久值;

        public abstract float GetStackingValue(string @event, DiziCombatUnit target);
        public abstract bool IsAllowStacking { get; }

        public abstract void OnTriggeredFunction(DiziCombatUnit target, DiziCombatUnit caster, CombatArgs args,
            BuffManager<DiziCombatUnit> buffManager, List<DiziCombatUnit> units);

        public abstract IEnumerable<CombatBuff> InstanceSelfBuffs(DiziCombatUnit caster, int round,
            BuffManager<DiziCombatUnit> buffMgr);

        public abstract IEnumerable<CombatBuff> InstanceTargetBuffs(DiziCombatUnit target, DiziCombatUnit caster,
            CombatArgs args, int round, BuffManager<DiziCombatUnit> buffMgr);
    }
}