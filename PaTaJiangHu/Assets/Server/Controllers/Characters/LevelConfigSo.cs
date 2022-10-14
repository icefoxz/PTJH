using System;
using UnityEngine;

namespace Server.Controllers.Characters
{
    [CreateAssetMenu(fileName = "LevelingConfig", menuName = "配置/升等配置")]
    internal class LevelConfigSo : ScriptableObject
    {
        [SerializeField] private float 力量成长倍率;
        [SerializeField] private float 敏捷成长倍率;
        [SerializeField] private float 血成长倍率;
        [SerializeField] private float 气成长倍率;
        [SerializeField] private float 内成长倍率;

        public enum Props
        {
            [InspectorName("力量")] Strength,
            [InspectorName("敏捷")] Agility,
            [InspectorName("血")] Hp,
            [InspectorName("气")] Tp,
            [InspectorName("内")] Mp
        }

        public float StrengthGrowingRate => 力量成长倍率;
        public float AgilityGrowingRate => 敏捷成长倍率;
        public float HpGrowingRate => 血成长倍率;
        public float TpGrowingRate => 气成长倍率;
        public float MpGrowingRate => 内成长倍率;

        public int GetLeveledValue(int level, int baseValue, Props prop)
        {
            if (level <= 0) throw new InvalidOperationException($"{prop}.等级不可以小于1!");
            return prop switch
            {
                Props.Strength => LevelingFormula(level, baseValue, StrengthGrowingRate),
                Props.Agility => LevelingFormula(level, baseValue, AgilityGrowingRate),
                Props.Hp => LevelingFormula(level, baseValue, HpGrowingRate),
                Props.Tp => LevelingFormula(level, baseValue, TpGrowingRate),
                Props.Mp => LevelingFormula(level, baseValue, MpGrowingRate),
                _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
            };
        }

        private int LevelingFormula(int level, int baseValue, float rate) => (int)((level - 1) * rate + baseValue);
    }
}
