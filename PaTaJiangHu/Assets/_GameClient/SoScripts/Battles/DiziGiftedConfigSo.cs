using System.Collections.Generic;
using AOT._AOT.Utls;
using GameClient.Modules.BattleM;
using UnityEngine;

namespace GameClient.SoScripts.Battles
{
    [CreateAssetMenu(fileName = "弟子属性天赋策略", menuName = "弟子/天赋/天赋策略配置")]
    public class DiziGiftedConfigSo : ScriptableObject
    {
        [SerializeField] private GiftedDodgeSo[] 闪避率;
        [SerializeField] private GiftedCritRateSo[] 会心触发率;
        [SerializeField] private GiftedHardRateSo[] 重击触发率;
        [SerializeField] private GiftedCritDamageRateSo[] 会心伤害比率;
        [SerializeField] private GiftedHardDamageRateSo[] 重击伤害比率;
        [SerializeField] private GiftedMpDamageRateSo[] 内力伤害转化率;
        [SerializeField] private GiftedMpArmorConvertSo[] 内力抵消转化率;
        private GiftedDodgeSo[] GiftedDodgeSos => 闪避率;
        private GiftedCritRateSo[] GiftedCritRateSos => 会心触发率;
        private GiftedHardRateSo[] GiftedHardRateSos => 重击触发率;
        private GiftedCritDamageRateSo[] GiftedCritDamageRateSos => 会心伤害比率;
        private GiftedHardDamageRateSo[] GiftedHardDamageRateSos => 重击伤害比率;
        private GiftedMpDamageRateSo[] GiftedMpDamageRateSos => 内力伤害转化率;
        private GiftedMpArmorConvertSo[] GiftedMpArmorConvertSos => 内力抵消转化率;

        public ICombatGifted GenerateDiziGifted()
        {
            var dodgeRateMax = 100 + GetRandomFromArray(GiftedDodgeSos);
            var critRateMax = 100 + GetRandomFromArray(GiftedCritRateSos);
            var hardRateMax = 100 + GetRandomFromArray(GiftedHardRateSos);
            var critDamageRateAddOn = 100 + GetRandomFromArray(GiftedCritDamageRateSos);
            var hardDamageRateAddOn = 100 + GetRandomFromArray(GiftedHardDamageRateSos);
            var mpDamageRate = 100 + GetRandomFromArray(GiftedMpDamageRateSos);
            var mpArmorRate = 100 + GetRandomFromArray(GiftedMpArmorConvertSos);
            return new CombatGifted(
                dodgeRateMax: dodgeRateMax, 
                critRateMax: critRateMax, 
                hardRateMax: hardRateMax, 
                critDamageRateMax: critDamageRateAddOn, 
                hardDamageRateMax: hardDamageRateAddOn, 
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
            public float CritDamageRate { get; }
            public float HardDamageRate { get; }
            public float MpDamageRate { get; }
            public float MpArmorRate { get; }

            public CombatGifted(
                float dodgeRateMax, 
                float critRateMax, 
                float hardRateMax, 
                float critDamageRateMax, 
                float hardDamageRateMax, 
                float mpDamageRate, 
                float mpArmorRate)
            {
                DodgeRateMax = dodgeRateMax;
                CritRateMax = critRateMax;
                HardRateMax = hardRateMax;
                CritDamageRate = critDamageRateMax;
                HardDamageRate = hardDamageRateMax;
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