using System;
using Server.Configs.Items;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Skills
{
    [CreateAssetMenu(fileName = "autoName", menuName = "战斗/武学/差值策略")]
    [Serializable]
    internal class CombatCompareStrategySo : AutoAtNamingObject, ISkillAttribute
    {
        public enum Settings
        {
            [InspectorName("重击触发")] HardRate,
            [InspectorName("重击倍率")] HardDamageRate,
            [InspectorName("会心触发")] CriticalRate,
            [InspectorName("闪避触发")] DogeRate
        }

        [SerializeField] private Settings _set;
        [SerializeField] private Combat.Compares 差值;
        [SerializeField] private float 校正;
        [SerializeField] private Combat.Calculate 计算;
        [SerializeField] private float 系数;
        [SerializeField] private ColorGrade 品级;
        [SerializeField] [TextArea] private string 说明;

        public Settings Set => _set;
        public Combat.Compares Compare => 差值;
        public Combat.Calculate Cal => 计算;
        public float Factor => 系数;
        public float Offset => 校正;
        public ColorGrade Grade => 品级;
        public string Intro => 说明;

        #region Calculate

        private float Calculation(CombatArgs arg)
        {
            return Cal switch
            {
                Combat.Calculate.Divide => DivideFactor(arg),
                Combat.Calculate.Multiply => MultiplyFactor(arg),
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
                Combat.Compares.Strength => dizi.Strength.Value,
                Combat.Compares.Agility => dizi.Agility.Value,
                Combat.Compares.Hp => dizi.Hp.Value,
                Combat.Compares.HpMax => dizi.Hp.Max,
                Combat.Compares.Mp => dizi.Mp.Value,
                Combat.Compares.MpMax => dizi.Mp.Max,
                _ => throw new ArgumentOutOfRangeException()
            };

        #endregion

        public float GetHardRate(CombatArgs arg) => Calculation(arg);
        public float GetHardDamageRatio(CombatArgs arg) => Calculation(arg);
        public float GetCriticalRate(CombatArgs arg) => Calculation(arg);
        public float GetDodgeRate(CombatArgs arg) => Calculation(arg);

        public ISkillAttribute GetCombatAttribute() => this;
    }
}