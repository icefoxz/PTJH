using System;
using System.Collections.Generic;
using Data;
using Server.Configs.Battles;
using UnityEngine;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "forceSo", menuName = "战斗/武学/内功")]
    public class ForceFieldSo : SkillFieldSo
    {
        [Header("负数为百分比")]
        [SerializeField] private int 内力使用;
        public override SkillType SkillType => SkillType.Force;
        private int MpUses => 内力使用;

        protected override IList<ICombatSet> CustomCombatSets()
        {
            return new List<ICombatSet>
            {
                new CombatSet(hardRate: null, 
                    hardDamageRatio: null, 
                    criticalRate: null, 
                    criticalMultiplier: null,
                    mpDamage: new List<Func<CombatArgs, float>> { MpUsesFromCaster }, 
                    mpCounteract: null, 
                    dodgeRate: null)
            };
        }

        private float MpUsesFromCaster(CombatArgs arg)
        {
            if (MpUses >= 0) return MpUses;
            var mp = MathF.Abs(MpUses) * arg.Caster.MaxMp * 0.01f;
            return mp;
        }
    }

}