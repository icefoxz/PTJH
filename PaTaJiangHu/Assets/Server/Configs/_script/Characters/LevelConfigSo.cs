using System;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Server.Configs.Characters
{
    [CreateAssetMenu(fileName = "LevelingConfig", menuName = "弟子/升等配置")]
    internal class LevelConfigSo : ScriptableObject
    {
        [SerializeField] private int[] 升等经验;
        [SerializeField] private LevelingConfig[] 力量成长;
        [SerializeField] private LevelingConfig[] 敏捷成长;
        [SerializeField] private LevelingConfig[] 血成长;
        [SerializeField] private LevelingConfig[] 内成长;
        private DiziProps[] _props;
        public DiziProps[] GetProsArray => _props ??= new[]
        {
            DiziProps.Strength, 
            DiziProps.Agility, 
            DiziProps.Hp, 
            DiziProps.Mp
        };
        
        private LevelingConfig[] StrengthGrowingRate => 力量成长;
        private LevelingConfig[] AgilityGrowingRate => 敏捷成长;
        private LevelingConfig[] HpGrowingRate => 血成长;
        private LevelingConfig[] MpGrowingRate => 内成长;
        private int[] UpgradeExp => 升等经验;
        public int MaxLevel => new[]
            { StrengthGrowingRate.Length, AgilityGrowingRate.Length, HpGrowingRate.Length, MpGrowingRate.Length }.Min();
        public int GetMaxExp(int level) => UpgradeExp[level - 1];
        private int LevelToIndex(int level) => level - 2;
        public int GetLeveledBonus(DiziProps prop,int baseValue ,int level)
        {
            if (level <= 0) throw new InvalidOperationException($"{prop}.等级不可以小于1!");
            if (level == 1) return 0;
            return prop switch
            {
                DiziProps.Strength => (int)((LevelingRatio(level, StrengthGrowingRate) - 1) * baseValue),
                DiziProps.Agility => (int)((LevelingRatio(level, AgilityGrowingRate) - 1) * baseValue),
                DiziProps.Hp => (int)((LevelingRatio(level, HpGrowingRate) - 1) * baseValue),
                DiziProps.Mp => (int)((LevelingRatio(level, MpGrowingRate) - 1) * baseValue),
                _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
            };
        }
        private float LevelingRatio(int level, LevelingConfig[] rates)
        {
            var index = LevelToIndex(level);
            if (rates.Length <= index)
                throw new InvalidOperationException($"等级已超过最大限制[{rates.Length + 1}],当前等级{level}！");
            return rates[index].Ratio;
        }

        [Serializable]
        private class LevelingConfig
        {
            private bool Rename()
            {
                _name = $"倍率{Ratio * 100}%";
                return true;
            }

            [ConditionalField(true,nameof(Rename))][ReadOnly][SerializeField] private string _name;
            [SerializeField] private float 倍率;
            public float Ratio => 倍率;
        }
    }
}
