using System;
using System.Collections.Generic;
using GameClient.Args;
using GameClient.Modules.BattleM;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameClient.SoScripts.Skills
{
    [CreateAssetMenu(fileName = "forceSo", menuName = "战斗/武学/内功")]
    public class ForceFieldSo : SkillFieldSo
    {
        [Header("负数为百分比")]
        [FormerlySerializedAs("内力使用")][SerializeField] private int 内力攻击;
        [ConditionalField(nameof(分开设置伤害占比), false)] [SerializeField] private int 内力护甲 = 0;
        [ConditionalField(nameof(分开设置伤害占比), false)] [FormerlySerializedAs("内力减免占比")][SerializeField][Range(0,100)]private int 伤害抵消占比 = 50;
        [ConditionalField(nameof(分开设置伤害占比), true)] [SerializeField] private int 抵消护甲 = -50;
        [SerializeField] private bool 分开设置伤害占比;
        [SerializeField] private int 伤害转化加成 = 0;
        [SerializeField] private int 抵消转化加成 = 0;

        private bool SeparateMpOffset => 分开设置伤害占比;
        //策划要求正数为内力护甲而负数为伤害抵消占比
        private int OffSetArmor => 抵消护甲;

        public override SkillType SkillType => SkillType.Force;

        private int MpUses => 内力攻击;
        private int MpDamageConvertRate => 伤害转化加成;
        private int MpArmor => 内力护甲;
        private int DamageMpArmorRate => 伤害抵消占比;
        private int MpArmorRateConvertRate => 抵消转化加成;

        protected override IList<ICombatSet> AdditionCombatSets()
        {
            return new List<ICombatSet>
            {
                new CombatSet(hardRate: null,
                    hardDamageRatio: null,
                    criticalRate: null,
                    criticalDamageRatio: null,
                    mpUses: new List<Func<CombatArgs, float>> { MpUsesFromCaster },
                    mpArmor: new List<Func<CombatArgs, float>> { GetMpArmor },
                    mpDamageCovertRateAddOn: new List<Func<CombatArgs, float>> { GetMpDamageConvertRateAddOn },
                    damageMpArmorRate: new List<Func<CombatArgs, float>> { GetDamageMpArmorRate },
                    mpArmorConvertRateAddOn: new List<Func<CombatArgs, float>> { GetArmorConvertRateAddOn },
                    dodgeRate: null,
                    selfBuffs: null,
                    targetBuffs: null)
            };
        }

        // 提供内力护甲值
        private float GetMpArmor(CombatArgs arg)
        {
            if (SeparateMpOffset) // 如果分开设置伤害占比, 直接返回护甲值(正数为值, 负数为百分比)
                return MpArmor > 0 ? MpArmor : arg.Caster.Mp.Max * MpArmor / 100f;
            // 如果不分开设置伤害占比, 则返回抵消护甲值, 但如果抵消护甲为负数将调用当前的内力值为抵消的值
            return OffSetArmor > 0 ? OffSetArmor : arg.Caster.Mp.Value;
        }

        // 提供一个内力减免占比, 让战斗控制器基于伤害去计算(伤害分散公式)
        // 正数为值, 而负数为百分比
        private float GetDamageMpArmorRate(CombatArgs arg)
        {
            if(!SeparateMpOffset) // 如果不分开设置伤害占比
            {
                // 如果是正数, 则为半数值(常规), 否则为抵消护甲的值来提供
                return OffSetArmor > 0 ? 50 : -OffSetArmor;
            }
            if (DamageMpArmorRate < 0)
                throw new InvalidOperationException($"({name})的内力减免占比不支持小于0");
            return DamageMpArmorRate;
        }

        // 提供内力与伤害的抵消转化率
        private float GetArmorConvertRateAddOn(CombatArgs arg) => MpArmorRateConvertRate;

        // 提供内力伤害转化率
        private float GetMpDamageConvertRateAddOn(CombatArgs arg) => MpDamageConvertRate;

        // 提供内力伤害
        private float MpUsesFromCaster(CombatArgs arg)
        {
            if (MpUses >= 0) return MpUses;
            var mp = MathF.Abs(MpUses) * arg.Caster.Mp.Max / 100f;
            return mp;
        }
    }

}