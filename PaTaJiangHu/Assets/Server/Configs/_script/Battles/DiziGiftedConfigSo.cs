using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utls;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "弟子属性天赋策略", menuName = "弟子/天赋/天赋策略配置")]
    public class DiziGiftedConfigSo : ScriptableObject
    {
        [SerializeField] private GiftedDodgeSo[] 闪避率;
        [SerializeField] private GiftedCritRateSo[] 会心触发率;
        [SerializeField] private GiftedHardRateSo[] 重击触发率;
        [SerializeField] private GiftedCritDamageRatioSo[] 会心伤害比率;
        [SerializeField] private GiftedHardDamageRatioSo[] 重击伤害比率;
        [SerializeField] private GiftedMpDamageRateSo[] 内力伤害转化率;
        [SerializeField] private GiftedMpArmorConvertSo[] 内力抵消转化率;
        private GiftedDodgeSo[] GiftedDodgeSos => 闪避率;
        private GiftedCritRateSo[] GiftedCritRateSos => 会心触发率;
        private GiftedHardRateSo[] GiftedHardRateSos => 重击触发率;
        private GiftedCritDamageRatioSo[] GiftedCritDamageRatioSos => 会心伤害比率;
        private GiftedHardDamageRatioSo[] GiftedHardDamageRatioSos => 重击伤害比率;
        private GiftedMpDamageRateSo[] GiftedMpDamageRateSos => 内力伤害转化率;
        private GiftedMpArmorConvertSo[] GiftedMpArmorConvertSos => 内力抵消转化率;

        public ICombatGifted GenerateDiziGifted()
        {
            var dodgeRateMax = 100 + GetRandomFromArray(GiftedDodgeSos);
            var critRateMax = 100 + GetRandomFromArray(GiftedCritRateSos);
            var hardRateMax = 100 + GetRandomFromArray(GiftedHardRateSos);
            var critDamageRatioMax = 3 + GetRandomFromArray(GiftedCritDamageRatioSos)/100f;
            var hardDamageRatioMax = 3 + GetRandomFromArray(GiftedHardDamageRatioSos)/100f;
            var mpDamageRate = 100 + GetRandomFromArray(GiftedMpDamageRateSos);
            var mpArmorRate = 100 + GetRandomFromArray(GiftedMpArmorConvertSos);
            return new CombatGifted(
                dodgeRateMax: dodgeRateMax, 
                critRateMax: critRateMax, 
                hardRateMax: hardRateMax, 
                critDamageRatioMax: critDamageRatioMax, 
                hardDamageRatioMax: hardDamageRatioMax, 
                mpDamageRate: mpDamageRate,
                mpArmorRate: mpArmorRate);
        }

        private static float GetRandomFromArray(IReadOnlyCollection<GiftedCfgSoBase> array) =>
            array.Count > 0 ? array.RandomPick().GetValue() : 0;

        private record CombatGifted : ICombatGifted
        {
            public float DodgeRateMax { get; }
            public float CritRateMax { get; }
            public float HardRateMax { get; }
            public float CritDamageRatioMax { get; }
            public float HardDamageRatioMax { get; }
            public float MpDamageRate { get; }
            public float MpArmorRate { get; }

            public CombatGifted(
                float dodgeRateMax, 
                float critRateMax, 
                float hardRateMax, 
                float critDamageRatioMax, 
                float hardDamageRatioMax, 
                float mpDamageRate, 
                float mpArmorRate)
            {
                DodgeRateMax = dodgeRateMax;
                CritRateMax = critRateMax;
                HardRateMax = hardRateMax;
                CritDamageRatioMax = critDamageRatioMax;
                HardDamageRatioMax = hardDamageRatioMax;
                MpDamageRate = mpDamageRate;
                MpArmorRate = mpArmorRate;
            }
        }
    }
    public abstract class GiftedCfgSoBase : ScriptableObject
    {
        public abstract float GetValue();
    }
}