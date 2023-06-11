using System;
using System.Collections.Generic;
using Data;
using Server.Configs.Battles;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "forceSo", menuName = "战斗/武学/内功")]
    public class ForceFieldSo : SkillFieldSo
    {
        [Header("直接填入内力的占比")][SerializeField]private int 内力减免占比 = 50;
        [Header("负数为百分比")]
        [FormerlySerializedAs("内力使用")][SerializeField] private int 内力使用;
        private int 内力伤害转化加成 = 0;// 暂时不让调
        private int 内力减免转化加成 = 0;// 暂时不让调

        public override SkillType SkillType => SkillType.Force;
        private int MpUses => 内力使用;
        private int MpDamageConvertRate => 内力伤害转化加成;
        private int MpArmorRate => 内力减免占比;
        private int MpArmorRateConvertRate => 内力减免转化加成;

        protected override IList<ICombatSet> AdditionCombatSets()
        {
            return new List<ICombatSet>
            {
                new CombatSet(hardRate: null, 
                    hardDamageRatio: null, 
                    criticalRate: null, 
                    criticalDamageRatio: null,
                    mpUses: new List<Func<CombatArgs, float>> { MpUsesFromCaster }, 
                    mpDamageCovertRateAddOn: new List<Func<CombatArgs, float>> {GetMpDamageConvertRateAddOn},
                    mpArmorRate: new List<Func<CombatArgs, float>> {GetMpArmorRate}, 
                    mpArmorConvertRateAddOn: new List<Func<CombatArgs, float>>{GetArmorConvertRateAddOn},
                    dodgeRate: null,
                    selfBuffs: null,
                    targetBuffs: null)
            };
        }

        // 提供一个内力减免占比, 让战斗控制器基于伤害去计算(伤害分散公式)
        // 正数为值, 而负数为百分比
        private float GetMpArmorRate(CombatArgs arg)
        {
            if (MpArmorRate < 0)
                throw new InvalidOperationException($"({name})的内力减免占比不可以小于0");
            return MpArmorRate;
        }

        // 提供内力与伤害的抵消转化率
        private float GetArmorConvertRateAddOn(CombatArgs arg) => MpArmorRateConvertRate;

        // 提供内力伤害转化率
        private float GetMpDamageConvertRateAddOn(CombatArgs arg) => MpDamageConvertRate;

        // 提供内力伤害
        private float MpUsesFromCaster(CombatArgs arg)
        {
            if (MpUses >= 0) return MpUses;
            var mp = MathF.Abs(MpUses) * arg.Caster.Mp.Max * 0.01f;
            return mp;
        }
    }

}